
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipRecoveryController : MonoBehaviour
{
    [Header("Nave")]
    [SerializeField] private Transform naveAnimada;

    [Header("Objetivo")]
    [SerializeField] private int orbsNecessarios = 3;

    private int orbsEntregues = 0;

    private readonly HashSet<EntityId> orbsRegistrados = new();
    
    private float tempoInicioHover;

    [SerializeField] private float duracaoEntradaFlutuacao = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioNave;
    [SerializeField] private AudioClip somPrimeiroOrb;
    [SerializeField] private AudioClip somSegundoOrb;
    [SerializeField] private AudioClip somTerceiroOrb;

    [Header("Vibracao - Primeiro orb")]
    [SerializeField] private float duracaoPrimeiraVibracao = 1.5f;
    [SerializeField] private float intensidadePrimeiraVibracao = 0.015f;
    [SerializeField] private float rotacaoPrimeiraVibracao = 0.25f;

    [Header("Vibracao - Segundo orb")]
    [SerializeField] private float duracaoSegundaVibracao = 3f;
    [SerializeField] private float intensidadeSegundaVibracao = 0.035f;
    [SerializeField] private float rotacaoSegundaVibracao = 0.6f;

    [Header("Vibracao - Terceiro orb")]
    [SerializeField] private float duracaoTerceiraVibracao = 2f;
    [SerializeField] private float intensidadeTerceiraVibracao = 0.06f;
    [SerializeField] private float rotacaoTerceiraVibracao = 1f;

    [Header("Configuracao das vibracoes")]
    [SerializeField] private float frequenciaVibracao = 18f;

    [Header("Decolagem")]
    [SerializeField] private Transform pontoDeVoo;
    [SerializeField] private float duracaoDecolagem = 5f;

    [Header("Flutuacao final")]
    [SerializeField] private float alturaFlutuacao = 0.04f;
    [SerializeField] private float velocidadeFlutuacao = 1.3f;
    
    [Header("Encerramento")]
    [SerializeField] private EndingPanelController endingPanelController;

    private int etapasProcessadas = 0;
    private bool processandoEtapas = false;
    private bool naveEmHover = false;

    private Vector3 posicaoHover;


    private void Awake()
    {
        orbsNecessarios = Mathf.Max(1, orbsNecessarios);

        if (naveAnimada == null)
        {
            Debug.LogError(
                "ShipRecoveryController: configure a nave animada.",
                this
            );
        }

        if (pontoDeVoo == null)
        {
            Debug.LogError(
                "ShipRecoveryController: configure o ponto de voo.",
                this
            );
        }
    }


    // Recebe a notificação de cada orb inserido na nave.
    public void RegistrarOrbEntregue(Orb orb)
    {
        if (orb == null || orbsEntregues >= orbsNecessarios)
            return;

        // Impede que o mesmo orb seja contabilizado novamente.
        if (!orbsRegistrados.Add(orb.GetEntityId()))
            return;

        orbsEntregues++;

        Debug.Log(
            $"Nave recebeu o orb {orbsEntregues}/{orbsNecessarios}."
        );

        // Consome o orb depois de sua chegada.
        Destroy(orb.gameObject);

        if (!processandoEtapas)
        {
            StartCoroutine(ProcessarEtapas());
        }
    }


    private IEnumerator ProcessarEtapas()
    {
        processandoEtapas = true;

        while (etapasProcessadas < orbsEntregues)
        {
            etapasProcessadas++;

            switch (etapasProcessadas)
            {
                // PRIMEIRO ORB
                case 1:

                    ReproduzirSom(somPrimeiroOrb);

                    yield return TremerNave(
                        duracaoPrimeiraVibracao,
                        intensidadePrimeiraVibracao,
                        rotacaoPrimeiraVibracao
                    );

                    break;


                // SEGUNDO ORB
                case 2:

                    ReproduzirSom(somSegundoOrb);

                    yield return TremerNave(
                        duracaoSegundaVibracao,
                        intensidadeSegundaVibracao,
                        rotacaoSegundaVibracao
                    );

                    break;


                // TERCEIRO ORB
                case 3:

                    ReproduzirSom(somTerceiroOrb);

                    // Última vibração antes da decolagem.
                    yield return TremerNave(
                        duracaoTerceiraVibracao,
                        intensidadeTerceiraVibracao,
                        rotacaoTerceiraVibracao
                    );

                    // Decolagem da única nave existente.
                    yield return Decolar();

                    // Registra exatamente onde a nave terminou a decolagem.
                    posicaoHover = naveAnimada.position;

                    // Inicia a contagem da flutuação.
                    tempoInicioHover = Time.time;

                    // Ativa a animação de flutuação.
                    naveEmHover = true;

                    Debug.Log(
                        "Nave recuperada e operacional!"
                    );
                    
                    // Inicia a contagem para o painel final.
                    endingPanelController.IniciarEncerramento();

                    break;
            }
        }

        processandoEtapas = false;
    }


    // Produz uma vibração mecânica temporária na nave.
    private IEnumerator TremerNave(
        float duracao,
        float amplitudePosicional,
        float amplitudeRotacional)
    {
        Vector3 posicaoOriginal = naveAnimada.position;
        Quaternion rotacaoOriginal = naveAnimada.rotation;

        float tempo = 0f;

        while (tempo < duracao)
        {
            float progresso = tempo / duracao;

            // Faz a vibração começar e terminar suavemente.
            float envelope = Mathf.Sin(progresso * Mathf.PI);

            float fase = tempo * frequenciaVibracao * Mathf.PI * 2f;

            Vector3 deslocamento = new Vector3(
                Mathf.Sin(fase),
                Mathf.Sin(fase * 1.17f + 0.8f),
                Mathf.Sin(fase * 0.83f + 1.7f)
            ) * amplitudePosicional * envelope;

            Vector3 angulos = new Vector3(
                Mathf.Sin(fase * 1.11f + 0.4f),
                Mathf.Sin(fase * 0.91f + 1.1f),
                Mathf.Sin(fase * 1.23f + 2.1f)
            ) * amplitudeRotacional * envelope;

            naveAnimada.SetPositionAndRotation(
                posicaoOriginal + deslocamento,
                rotacaoOriginal * Quaternion.Euler(angulos)
            );

            tempo += Time.deltaTime;

            yield return null;
        }

        // Restaura exatamente a posição e a rotação anteriores.
        naveAnimada.SetPositionAndRotation(
            posicaoOriginal,
            rotacaoOriginal
        );
    }


    // Faz a nave subir e corrigir sua orientação.
    private IEnumerator Decolar()
    {
        Vector3 posicaoInicial = naveAnimada.position;
        Quaternion rotacaoInicial = naveAnimada.rotation;

        Vector3 posicaoFinal = pontoDeVoo.position;
        Quaternion rotacaoFinal = pontoDeVoo.rotation;

        float tempo = 0f;

        while (tempo < duracaoDecolagem)
        {
            float progresso = Mathf.Clamp01(
                tempo / duracaoDecolagem
            );

            float suavizado = Mathf.SmoothStep(
                0f,
                1f,
                progresso
            );

            Vector3 novaPosicao = Vector3.Lerp(
                posicaoInicial,
                posicaoFinal,
                suavizado
            );

            Quaternion novaRotacao = Quaternion.Slerp(
                rotacaoInicial,
                rotacaoFinal,
                suavizado
            );

            naveAnimada.SetPositionAndRotation(
                novaPosicao,
                novaRotacao
            );

            tempo += Time.deltaTime;

            yield return null;
        }

        naveAnimada.SetPositionAndRotation(
            posicaoFinal,
            rotacaoFinal
        );
    }


    private void ReproduzirSom(AudioClip som)
    {
        if (audioNave != null && som != null)
        {
            audioNave.PlayOneShot(som);
        }
    }


    // Mantém uma pequena flutuação após a recuperação completa.
    
    private void Update()
    {
        if (!naveEmHover || naveAnimada == null)
            return;

        // Tempo transcorrido desde o início da flutuação.
        float tempoDecorrido = Time.time - tempoInicioHover;

        // Aumenta gradualmente a intensidade da flutuação.
        float progressoEntrada = Mathf.Clamp01(
            tempoDecorrido / duracaoEntradaFlutuacao
        );

        float intensidade = Mathf.SmoothStep(
            0f,
            1f,
            progressoEntrada
        );

        // Movimento vertical suave.
        float deslocamentoY =
            Mathf.Sin(tempoDecorrido * velocidadeFlutuacao)
            * alturaFlutuacao
            * intensidade;

        // Atualiza a posição da nave.
        naveAnimada.position = posicaoHover +
                               Vector3.up * deslocamentoY;
    }

}
