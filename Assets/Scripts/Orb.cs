
using System;
using UnityEngine;

public enum TipoCorOrb
{
    Verde,
    Laranja,
    Violeta
}

public class Orb : MonoBehaviour
{
    [Header("Identidade do orb")]
    [SerializeField] private TipoCorOrb corDoOrb;

    [Header("Efeito visual")]
    [SerializeField] private GameObject efeitoAtmosfericoGigante;

    [Header("Transporte")]
    [SerializeField] private float velocidadeAteNave = 1.5f;
    [SerializeField] private float distanciaDeChegada = 0.05f;

    [Header("Audio - Coleta")]
    [SerializeField] private AudioSource audioColeta;
    [SerializeField] private AudioClip somColeta;

    [Header("Audio - Orb segurado")]
    [SerializeField] private AudioSource audioOrbSegurado;
    [SerializeField] private AudioClip somOrbSegurado;


    private enum EstadoOrb
    {
        NoCenario,
        Segurado,
        EmTransito,
        Entregue
    }

    private EstadoOrb estado = EstadoOrb.NoCenario;

    private Collider[] orbColliders;

    private OrbAnimation animacaoVisual;

    private Transform pontoDeApoio;
    private Transform destino;

    private Action<Orb> aoChegar;

    public TipoCorOrb Cor => corDoOrb;

    public bool PodeSerPego => estado == EstadoOrb.NoCenario;


    private void Awake()
    {
        // Obtem a animacao que esta no objeto visual filho.
        animacaoVisual = GetComponentInChildren<OrbAnimation>(true);

        orbColliders = GetComponentsInChildren<Collider>(true);

        if (efeitoAtmosfericoGigante != null)
        {
            efeitoAtmosfericoGigante.SetActive(false);
        }

        if (animacaoVisual == null)
        {
            Debug.LogWarning(
                $"{name}: nenhum OrbAnimation foi encontrado nos filhos.",
                this
            );
        }
    }


    // Chamado quando o jogador pega o orb.
    public bool Pegar(Transform novoPontoDeApoio)
    {
        if (!PodeSerPego || novoPontoDeApoio == null)
            return false;

        estado = EstadoOrb.Segurado;

        pontoDeApoio = novoPontoDeApoio;

        // Reproduz o som de coleta uma unica vez.
        if (audioColeta != null && somColeta != null)
        {
            audioColeta.PlayOneShot(somColeta);
        }

        // Inicia o zumbido continuo enquanto o orb estiver segurado.
        if (audioOrbSegurado != null && somOrbSegurado != null)
        {
            audioOrbSegurado.clip = somOrbSegurado;
            audioOrbSegurado.loop = true;
            audioOrbSegurado.Play();
        }

        // Interrompe somente a flutuacao.
        // A rotacao continua ativa.
        if (animacaoVisual != null)
        {
            animacaoVisual.DefinirFlutuacao(false);
        }

        AtivarColisores(false);

        // Torna o orb filho do HoldPoint,
        // preservando escala e transformacao mundial.
        transform.SetParent(pontoDeApoio, true);

        // Posiciona diante da camera.
        transform.position = pontoDeApoio.position;

        // Ativa o efeito atmosferico.
        if (efeitoAtmosfericoGigante != null)
        {
            efeitoAtmosfericoGigante.SetActive(true);
        }

        return true;
    }


    // Chamado quando o jogador envia o orb para a nave.
    public bool EnviarParaNave(
        Transform pontoDeInsercao,
        Action<Orb> aoChegarAoDestino)
    {
        if (estado != EstadoOrb.Segurado ||
            pontoDeInsercao == null)
        {
            return false;
        }

        // Interrompe imediatamente o som continuo.
        PararSomSegurado();

        estado = EstadoOrb.EmTransito;

        // Remove o orb da hierarquia da camera,
        // preservando sua transformacao mundial.
        transform.SetParent(null, true);

        pontoDeApoio = null;

        destino = pontoDeInsercao;
        aoChegar = aoChegarAoDestino;

        Debug.Log(
            $"[Orb] {name}: iniciou transporte em direcao a {destino.name}."
        );

        return true;
    }


    // Interrompe o som continuo do orb.
    private void PararSomSegurado()
    {
        if (audioOrbSegurado != null && audioOrbSegurado.isPlaying)
        {
            audioOrbSegurado.Stop();
        }
    }


    private void LateUpdate()
    {
        // Mantem o orb diante da camera enquanto estiver segurado.
        if (estado == EstadoOrb.Segurado &&
            pontoDeApoio != null)
        {
            transform.position = pontoDeApoio.position;
        }
    }


    private void Update()
    {
        if (estado != EstadoOrb.EmTransito)
            return;

        if (destino == null)
        {
            Debug.LogWarning(
                $"{name}: o destino do orb nao existe mais.",
                this
            );

            return;
        }

        // Movimento suave em direcao a nave.
        transform.position = Vector3.MoveTowards(
            transform.position,
            destino.position,
            velocidadeAteNave * Time.deltaTime
        );

        // Verifica se o orb chegou ao destino.
        float distanciaQuadrada =
            (transform.position - destino.position).sqrMagnitude;

        if (distanciaQuadrada <=
            distanciaDeChegada * distanciaDeChegada)
        {
            transform.position = destino.position;

            estado = EstadoOrb.Entregue;

            // Interrompe as animacoes do orb.
            if (animacaoVisual != null)
            {
                animacaoVisual.PararAnimacoes();
            }

            // Desativa o efeito atmosferico gigante.
            if (efeitoAtmosfericoGigante != null)
            {
                efeitoAtmosfericoGigante.SetActive(false);
            }

            Action<Orb> callback = aoChegar;
            aoChegar = null;

            Debug.Log($"[Orb] {name}: chegou ao destino!");

            callback?.Invoke(this);
        }
    }


    private void AtivarColisores(bool ativo)
    {
        foreach (Collider col in orbColliders)
        {
            if (col != null)
            {
                col.enabled = ativo;
            }
        }
    }
}
