
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
        // Obtém a animação que está no objeto visual filho.
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

        // Interrompe somente a flutuação.
        // A rotação continua ativa.
        if (animacaoVisual != null)
        {
            animacaoVisual.DefinirFlutuacao(false);
        }

        AtivarColisores(false);

        // Torna o orb filho do HoldPoint,
        // preservando escala e transformação mundial.
        transform.SetParent(pontoDeApoio, true);

        // Posiciona diante da câmera.
        transform.position = pontoDeApoio.position;

        // Ativa o efeito atmosférico.
        if (efeitoAtmosfericoGigante != null)
        {
            efeitoAtmosfericoGigante.SetActive(true);
        }

        return true;
    }


    // Chamado quando o jogador solta o orb.
    public bool EnviarParaNave(
        Transform pontoDeInsercao,
        Action<Orb> aoChegarAoDestino)
    {
        if (estado != EstadoOrb.Segurado ||
            pontoDeInsercao == null)
        {
            return false;
        }

        estado = EstadoOrb.EmTransito;

        // Remove o orb da hierarquia da câmera,
        // preservando sua transformação mundial.
        transform.SetParent(null, true);

        pontoDeApoio = null;

        destino = pontoDeInsercao;
        aoChegar = aoChegarAoDestino;

        Debug.Log(
            $"[Orb] {name}: iniciou transporte em direção a {destino.name}."
        );

        return true;
    }


    private void LateUpdate()
    {
        // Mantém o orb diante da câmera enquanto estiver segurado.
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
                $"{name}: o destino do orb não existe mais.",
                this
            );

            return;
        }

        // Movimento suave em direção à nave.
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

            // Interrompe as animações do orb.
            if (animacaoVisual != null)
            {
                animacaoVisual.PararAnimacoes();
            }

            // Desativa o efeito atmosférico gigante.
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
