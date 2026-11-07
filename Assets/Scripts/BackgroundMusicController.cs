
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicController : MonoBehaviour
{
    [Header("Playlist")]
    [SerializeField] private AudioClip[] musicas;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)]
    private float volumeMusica = 0.25f;

    [Header("Transicoes")]
    [SerializeField] private float duracaoFade = 1.5f;

    [Header("Aleatoriedade")]
    [SerializeField] private bool evitarMesmaOrdemDaPartidaAnterior = true;

    private AudioSource audioMusica;

    private System.Random geradorAleatorio;

    private AudioClip ultimaMusicaTocada;

    private const string CHAVE_ULTIMA_ORDEM =
        "AlienFuel_OrdemMusicasAnterior";


    private void Awake()
    {
        audioMusica = GetComponent<AudioSource>();

        audioMusica.playOnAwake = false;
        audioMusica.loop = false;
        audioMusica.spatialBlend = 0f;

        audioMusica.volume = 0f;

        geradorAleatorio = new System.Random();
    }


    private void Start()
    {
        List<AudioClip> faixasValidas = new List<AudioClip>();

        if (musicas == null || musicas.Length == 0)
        {
            Debug.LogError(
                "BackgroundMusicController: nenhuma musica foi configurada.",
                this
            );

            return;
        }

        foreach (AudioClip musica in musicas)
        {
            if (musica != null)
            {
                faixasValidas.Add(musica);
            }
        }

        if (faixasValidas.Count == 0)
        {
            Debug.LogError(
                "BackgroundMusicController: todas as faixas estao vazias.",
                this
            );

            return;
        }

        StartCoroutine(TocarPlaylist(faixasValidas));
    }


    private IEnumerator TocarPlaylist(List<AudioClip> faixas)
    {
        string ordemAnterior = PlayerPrefs.GetString(
            CHAVE_ULTIMA_ORDEM,
            ""
        );

        while (true)
        {
            // Cria uma nova ordem aleatoria.
            List<AudioClip> ordem = CriarNovaOrdem(
                faixas,
                ordemAnterior
            );

            // Registra a ordem escolhida.
            ordemAnterior = CriarChaveOrdem(ordem);

            PlayerPrefs.SetString(
                CHAVE_ULTIMA_ORDEM,
                ordemAnterior
            );

            PlayerPrefs.Save();

            // Reproduz todas as musicas da ordem sorteada.
            foreach (AudioClip faixa in ordem)
            {
                yield return TocarFaixa(faixa);

                ultimaMusicaTocada = faixa;
            }
        }
    }


    // Sorteia uma ordem sem repetir faixas no mesmo ciclo.
    private List<AudioClip> CriarNovaOrdem(
        List<AudioClip> faixas,
        string ordemAnterior)
    {
        List<AudioClip> ordem;

        for (int tentativa = 0; tentativa < 100; tentativa++)
        {
            ordem = new List<AudioClip>(faixas);

            Embaralhar(ordem);

            // Evita repetir a ultima musica do ciclo anterior.
            bool repeteUltimaMusica =
                ordem.Count > 1 &&
                ordem[0] == ultimaMusicaTocada;

            // Evita repetir a sequencia completa da partida anterior.
            bool repeteOrdemAnterior =
                evitarMesmaOrdemDaPartidaAnterior &&
                ordem.Count > 1 &&
                CriarChaveOrdem(ordem) == ordemAnterior;

            if (!repeteUltimaMusica && !repeteOrdemAnterior)
            {
                return ordem;
            }
        }

        // Caso excepcional: gera uma ordem alternativa.
        ordem = new List<AudioClip>(faixas);

        Embaralhar(ordem);

        if (ordem.Count > 1 && ordem[0] == ultimaMusicaTocada)
        {
            AudioClip temporaria = ordem[0];
            ordem[0] = ordem[1];
            ordem[1] = temporaria;
        }

        return ordem;
    }


    // Embaralhamento Fisher-Yates.
    private void Embaralhar(List<AudioClip> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = geradorAleatorio.Next(i + 1);

            AudioClip temporaria = lista[i];

            lista[i] = lista[j];
            lista[j] = temporaria;
        }
    }


    // Cria uma identificacao da ordem das musicas.
    private string CriarChaveOrdem(List<AudioClip> ordem)
    {
        string chave = "";

        foreach (AudioClip musica in ordem)
        {
            chave += musica.name + "|";
        }

        return chave;
    }


    // Reproduz uma faixa com transicoes suaves.
    private IEnumerator TocarFaixa(AudioClip faixa)
    {
        audioMusica.Stop();

        audioMusica.clip = faixa;
        audioMusica.loop = false;
        audioMusica.volume = 0f;

        audioMusica.Play();

        Debug.Log(
            $"Trilha sonora iniciada: {faixa.name}"
        );

        float fade = Mathf.Clamp(
            duracaoFade,
            0f,
            faixa.length / 2f
        );

        if (fade > 0f)
        {
            // Aumenta gradualmente o volume.
            yield return FadeVolume(
                0f,
                volumeMusica,
                fade
            );

            // Aguarda a parte central da musica.
            float tempoCentral = faixa.length - 2f * fade;

            if (tempoCentral > 0f)
            {
                yield return new WaitForSecondsRealtime(
                    tempoCentral
                );
            }

            // Diminui gradualmente o volume.
            yield return FadeVolume(
                volumeMusica,
                0f,
                fade
            );
        }
        else
        {
            yield return new WaitForSecondsRealtime(
                faixa.length
            );
        }

        audioMusica.Stop();
    }


    // Controla o fade de entrada e de saida.
    private IEnumerator FadeVolume(
        float volumeInicial,
        float volumeFinal,
        float duracao)
    {
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracao
            );

            audioMusica.volume = Mathf.Lerp(
                volumeInicial,
                volumeFinal,
                progresso
            );

            yield return null;
        }

        audioMusica.volume = volumeFinal;
    }
}
