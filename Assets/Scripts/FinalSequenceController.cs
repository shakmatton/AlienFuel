
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FinalSequenceController : MonoBehaviour
{
    private enum EstadoSequencia
    {
        Aguardando,
        DescendoDisco,
        AguardandoInteracaoDisco,
        SurgindoHolograma,
        AguardandoInteracaoHolograma,
        AmpliandoNave,
        RetornandoDisco,
        ObservandoNave,
        Concluido
    }

    [Header("Referências principais")]
    [SerializeField] private Transform naveParaAmpliar;
    [SerializeField] private EndingPanelController endingPanelController;

    [Header("Colliders da zona de liberação")]
    [SerializeField] private Transform zonaLiberacaoOrbs;
    
    [Header("Jogador e interação")]
    [SerializeField] private Transform cameraJogador;
    [SerializeField] private InputActionReference acaoInteragir;
    [SerializeField] private TMP_Text textoAcaoFinal;
    [SerializeField] private float distanciaMaximaInteracao = 3f;

    [Header("Disco holográfico")]
    [SerializeField] private GameObject discoProjetor;
    [SerializeField] private Transform pontoNascimentoDisco;
    [SerializeField] private Transform pontoChegadaDisco;
    [SerializeField] private Collider colliderDisco;

    [SerializeField] private float atrasoAntesDeAparecer = 1f;
    [SerializeField] private float duracaoDescidaDisco = 3f;
    [SerializeField] private float duracaoSubidaDisco = 3f;
    
    [Header("Subida final até desaparecer")]
    [SerializeField] private float duracaoSubidaFinal = 15f;

    [Header("Holograma da nave")]
    [SerializeField] private Transform hologramaNave;
    [SerializeField] private Collider colliderHolograma;
    [SerializeField] private float duracaoSurgimentoHolograma = 1.5f;

    [Header("Ampliação da nave")]
    [SerializeField] private float multiplicadorEscalaNave = 2f;
    [SerializeField] private float duracaoAmpliacaoNave = 2.5f;

    [SerializeField] private ShipRecoveryController shipRecoveryController;
    [SerializeField] private float elevacaoExtraNave = 1.5f;

    [Header("Encerramento")]
    [SerializeField] private float tempoObservacaoNave = 5f;

    private EstadoSequencia estado = EstadoSequencia.Aguardando;

    private Vector3 escalaNormalDisco;
    private Vector3 escalaNormalHolograma;

    private bool sequenciaIniciada = false;


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Awake()
    {
        if (discoProjetor != null)
        {
            escalaNormalDisco = discoProjetor.transform.localScale;
            discoProjetor.SetActive(false);
        }

        if (hologramaNave != null)
        {
            escalaNormalHolograma = hologramaNave.localScale;
            hologramaNave.gameObject.SetActive(false);
        }

        if (colliderDisco != null)
            colliderDisco.enabled = false;

        if (colliderHolograma != null)
            colliderHolograma.enabled = false;

        AtualizarPrompt("");

        if (cameraJogador == null && Camera.main != null)
        {
            cameraJogador = Camera.main.transform;
        }
    }


    // =========================================================
    // INÍCIO DA SEQUÊNCIA
    // =========================================================

    public void IniciarSequenciaFinal()
    {
        if (sequenciaIniciada)
            return;

        if (naveParaAmpliar == null ||
            shipRecoveryController == null ||
            endingPanelController == null ||
            cameraJogador == null ||
            discoProjetor == null ||
            pontoNascimentoDisco == null ||
            pontoChegadaDisco == null ||
            colliderDisco == null ||
            hologramaNave == null ||
            colliderHolograma == null ||
            acaoInteragir == null ||
            acaoInteragir.action == null ||
            zonaLiberacaoOrbs == null)
        {
            Debug.LogError(
                "FinalSequenceController: existem referências obrigatórias não configuradas.",
                this
            );

            return;
        }

        sequenciaIniciada = true;

        // Desativa somente os colliders do OrbReleaseZone.
        DesativarCollidersZonaLiberacao();

        // Inicia a sequência normalmente.
        StartCoroutine(AparecerEDescerDisco());
    }


    // =========================================================
    // VERIFICAÇÃO DA MIRA E DAS INTERAÇÕES
    // =========================================================

    private void Update()
    {
        switch (estado)
        {
            case EstadoSequencia.AguardandoInteracaoDisco:
            {
                bool mirandoDisco =
                    MirandoNoAlvo(discoProjetor.transform);

                AtualizarPrompt(mirandoDisco ? "Ative!" : "");

                if (mirandoDisco && InteracaoPressionada())
                {
                    AtivarHolograma();
                }

                break;
            }

            case EstadoSequencia.AguardandoInteracaoHolograma:
            {
                bool mirandoHolograma =
                    MirandoNoAlvo(hologramaNave);

                AtualizarPrompt(mirandoHolograma ? "Interaja!" : "");

                if (mirandoHolograma && InteracaoPressionada())
                {
                    IniciarAmpliacaoNave();
                }

                break;
            }
        }
    }


    // =========================================================
    // 1. DISCO NASCE NO ALTO E DESCE
    // =========================================================

    private IEnumerator AparecerEDescerDisco()
    {
        estado = EstadoSequencia.DescendoDisco;

        if (atrasoAntesDeAparecer > 0f)
        {
            yield return new WaitForSecondsRealtime(
                atrasoAntesDeAparecer
            );
        }

        Transform discoTransform = discoProjetor.transform;

        // Posiciona o disco no ponto alto de nascimento.
        discoTransform.SetPositionAndRotation(
            pontoNascimentoDisco.position,
            pontoNascimentoDisco.rotation
        );

        discoTransform.localScale = escalaNormalDisco;

        // O holograma continua escondido.
        hologramaNave.gameObject.SetActive(false);

        colliderDisco.enabled = false;
        colliderHolograma.enabled = false;

        discoProjetor.SetActive(true);

        // Faz o disco descer até sua posição definitiva.
        yield return MoverDiscoAte(
            pontoChegadaDisco,
            duracaoDescidaDisco
        );

        // Agora o disco pode ser selecionado pela mira.
        colliderDisco.enabled = true;

        estado = EstadoSequencia.AguardandoInteracaoDisco;

        Debug.Log("Disco holográfico chegou à posição de interação.");
    }


    // =========================================================
    // 2. PRIMEIRA INTERAÇÃO: ATIVAR O PROJETOR
    // =========================================================

    private void AtivarHolograma()
    {
        colliderDisco.enabled = false;

        AtualizarPrompt("");

        estado = EstadoSequencia.SurgindoHolograma;

        StartCoroutine(AparecerHolograma());
    }


    // =========================================================
    // 3. HOLOGRAMA SURGE SUAVEMENTE
    // =========================================================

    private IEnumerator AparecerHolograma()
    {
        hologramaNave.gameObject.SetActive(true);

        // Começa praticamente invisível.
        Vector3 escalaInicial = escalaNormalHolograma * 0.001f;

        hologramaNave.localScale = escalaInicial;

        float tempo = 0f;

        while (tempo < duracaoSurgimentoHolograma)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoSurgimentoHolograma
            );

            float suave = Mathf.SmoothStep(
                0f,
                1f,
                progresso
            );

            hologramaNave.localScale = Vector3.Lerp(
                escalaInicial,
                escalaNormalHolograma,
                suave
            );

            yield return null;
        }

        hologramaNave.localScale = escalaNormalHolograma;

        // Só agora o holograma pode ser selecionado.
        colliderHolograma.enabled = true;

        estado = EstadoSequencia.AguardandoInteracaoHolograma;

        Debug.Log("Holograma pronto para interação.");
    }


    // =========================================================
    // 4. SEGUNDA INTERAÇÃO: AMPLIAR NAVE E HOLOGRAMA
    // =========================================================

    private void IniciarAmpliacaoNave()
    {
        colliderHolograma.enabled = false;

        AtualizarPrompt("");

        estado = EstadoSequencia.AmpliandoNave;

        StartCoroutine(AmpliarNaveEHolograma());
    }


    private IEnumerator AmpliarNaveEHolograma()
    {
        Vector3 escalaInicialNave = naveParaAmpliar.localScale;

        Vector3 escalaFinalNave =
            escalaInicialNave * multiplicadorEscalaNave;

        Vector3 escalaInicialHolograma = hologramaNave.localScale;

        Vector3 escalaFinalHolograma =
            escalaInicialHolograma * multiplicadorEscalaNave;

        float tempo = 0f;
        float elevacaoAplicada = 0f;

        while (tempo < duracaoAmpliacaoNave)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoAmpliacaoNave
            );

            float suave = Mathf.SmoothStep(
                0f,
                1f,
                progresso
            );
            
            // Eleva gradualmente a base da flutuação da nave.
            float elevacaoDesejada = elevacaoExtraNave * suave;

            float incrementoAltura = elevacaoDesejada - elevacaoAplicada;

            shipRecoveryController.AjustarAlturaBaseHover(incrementoAltura);

            elevacaoAplicada = elevacaoDesejada;

            // Amplia a nave real.
            naveParaAmpliar.localScale = Vector3.Lerp(
                escalaInicialNave,
                escalaFinalNave,
                suave
            );

            // Amplia o holograma proporcionalmente.
            hologramaNave.localScale = Vector3.Lerp(
                escalaInicialHolograma,
                escalaFinalHolograma,
                suave
            );

            yield return null;
        }

        naveParaAmpliar.localScale = escalaFinalNave;
        hologramaNave.localScale = escalaFinalHolograma;

        StartCoroutine(RetornarDiscoAoCeu());
    }


    // =========================================================
    // 5. DISCO E HOLOGRAMA RETORNAM AO PONTO ALTO
    // =========================================================

    
    
    private IEnumerator RetornarDiscoAoCeu()
    {
        estado = EstadoSequencia.RetornandoDisco;

        colliderDisco.enabled = false;
        colliderHolograma.enabled = false;

        // Começa as duas tarefas simultaneamente.
        estado = EstadoSequencia.ObservandoNave;

        StartCoroutine(SubirDiscoAteSumir());

        StartCoroutine(AguardarEncerramento());

        yield break;
    }


    
    private IEnumerator SubirDiscoAteSumir()
    {
        Transform discoTransform = discoProjetor.transform;

        // Calcula a velocidade média utilizada
        // na subida anterior.
        float distanciaReferencia = Vector3.Distance(
            pontoChegadaDisco.position,
            pontoNascimentoDisco.position
        );

        float duracaoReferencia = Mathf.Max(
            duracaoSubidaDisco,
            0.01f
        );

        float velocidadeSubida =
            distanciaReferencia / duracaoReferencia;

        float tempo = 0f;

        Debug.Log("Disco e holograma iniciaram a subida final.");

        // Mantém a subida durante o tempo configurado.
        while (tempo < duracaoSubidaFinal)
        {
            float delta = Time.unscaledDeltaTime;

            discoTransform.position +=
                Vector3.up * velocidadeSubida * delta;

            tempo += delta;

            yield return null;
        }

        // Desaparece somente depois dos 15 segundos.
        discoProjetor.SetActive(false);

        estado = EstadoSequencia.Concluido;

        Debug.Log("Disco e holograma desapareceram. Sequência concluída.");
    }



    // =========================================================
    // 6. ESPERA E EXIBE O PAINEL FINAL
    // =========================================================

    private IEnumerator AguardarEncerramento()
    {
        yield return new WaitForSecondsRealtime(
            tempoObservacaoNave
        );

        endingPanelController.ExibirEncerramentoAgora();

        Debug.Log("Painel final exibido.");
    }


    // =========================================================
    // ANIMAÇÃO DE MOVIMENTO DO DISCO
    // =========================================================

    private IEnumerator MoverDiscoAte(
        Transform destino,
        float duracao)
    {
        Transform discoTransform = discoProjetor.transform;

        Vector3 posicaoInicial = discoTransform.position;
        Quaternion rotacaoInicial = discoTransform.rotation;

        Vector3 posicaoFinal = destino.position;
        Quaternion rotacaoFinal = destino.rotation;

        if (duracao <= 0f)
        {
            discoTransform.SetPositionAndRotation(
                posicaoFinal,
                rotacaoFinal
            );

            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracao
            );

            float suave = Mathf.SmoothStep(
                0f,
                1f,
                progresso
            );

            discoTransform.position = Vector3.Lerp(
                posicaoInicial,
                posicaoFinal,
                suave
            );

            discoTransform.rotation = Quaternion.Slerp(
                rotacaoInicial,
                rotacaoFinal,
                suave
            );

            yield return null;
        }

        discoTransform.SetPositionAndRotation(
            posicaoFinal,
            rotacaoFinal
        );
    }
    
    
    private void DesativarCollidersZonaLiberacao()
    {
        if (zonaLiberacaoOrbs == null)
        {
            Debug.LogError(
                "FinalSequenceController: configure o OrbReleaseZone.",
                this
            );

            return;
        }

        // Busca todos os colliders do OrbReleaseZone
        // e de seus objetos filhos.
        Collider[] colliders =
            zonaLiberacaoOrbs.GetComponentsInChildren<Collider>(true);

        int quantidadeDesativada = 0;

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            if (col.enabled)
            {
                col.enabled = false;
                quantidadeDesativada++;
            }
        }

        Debug.Log(
            $"OrbReleaseZone: {quantidadeDesativada} colliders desativados.",
            this
        );
    }



    // =========================================================
    // FUNÇÕES AUXILIARES
    // =========================================================

    private bool MirandoNoAlvo(Transform alvo)
    {
        if (cameraJogador == null || alvo == null)
            return false;

        if (Physics.Raycast(
            cameraJogador.position,
            cameraJogador.forward,
            out RaycastHit resultado,
            distanciaMaximaInteracao,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide))
        {
            Transform atingido = resultado.collider.transform;

            return atingido == alvo ||
                   atingido.IsChildOf(alvo);
        }

        return false;
    }


    private bool InteracaoPressionada()
    {
        return acaoInteragir != null &&
               acaoInteragir.action != null &&
               acaoInteragir.action.WasPressedThisFrame();
    }


    private void AtualizarPrompt(string mensagem)
    {
        if (textoAcaoFinal == null)
            return;

        bool mostrar = !string.IsNullOrEmpty(mensagem);

        textoAcaoFinal.text = mensagem;

        textoAcaoFinal.gameObject.SetActive(mostrar);
    }
}
