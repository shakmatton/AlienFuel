
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

    [Header("Animação")]
    [SerializeField] private Pebbles animacaoFlutuacao;

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
    private Rigidbody orbRigidbody;

    private Transform destino;
    private Action<Orb> aoChegar;

    public TipoCorOrb Cor => corDoOrb;

    public bool PodeSerPego => estado == EstadoOrb.NoCenario;


    private void Awake()
    {
        orbColliders = GetComponentsInChildren<Collider>();
        orbRigidbody = GetComponent<Rigidbody>();

        if (orbRigidbody != null)
        {
            orbRigidbody.isKinematic = true;
            orbRigidbody.useGravity = false;
        }

        // O efeito gigante começa desativado.
        if (efeitoAtmosfericoGigante != null)
        {
            efeitoAtmosfericoGigante.SetActive(false);
        }

        if (animacaoFlutuacao == null)
        {
            Debug.LogWarning(
                $"{name}: associe o componente Pebbles no Inspector.",
                this
            );
        }
    }


    // Chamado quando o jogador pega o orb.
    public bool Pegar(Transform pontoDeApoio)
    {
        if (!PodeSerPego || pontoDeApoio == null)
            return false;

        // Interrompe somente a flutuação deste orb.
        if (animacaoFlutuacao != null)
        {
            animacaoFlutuacao.enabled = false;
        }

        estado = EstadoOrb.Segurado;

        // Impede colisões durante o transporte.
        AtivarColisores(false);

        // Preserva o tamanho original ao trocar de pai.
        transform.SetParent(pontoDeApoio, true);

        // Posiciona o orb diante da câmera.
        transform.position = pontoDeApoio.position;
        transform.rotation = pontoDeApoio.rotation;

        // Ativa o efeito visual gigante, caso esteja configurado.
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

        // Desprende o orb da câmera, preservando sua escala.
        transform.SetParent(null, true);

        destino = pontoDeInsercao;
        aoChegar = aoChegarAoDestino;

        return true;
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

        // Deslocamento suave até a entrada da nave.
        transform.position = Vector3.MoveTowards(
            transform.position,
            destino.position,
            velocidadeAteNave * Time.deltaTime
        );

        float distanciaQuadrada =
            (transform.position - destino.position).sqrMagnitude;

        if (distanciaQuadrada <=
            distanciaDeChegada * distanciaDeChegada)
        {
            transform.position = destino.position;

            estado = EstadoOrb.Entregue;

            // Desativa o efeito gigante quando o orb chega à nave.
            if (efeitoAtmosfericoGigante != null)
            {
                efeitoAtmosfericoGigante.SetActive(false);
            }

            Action<Orb> callback = aoChegar;
            aoChegar = null;

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
