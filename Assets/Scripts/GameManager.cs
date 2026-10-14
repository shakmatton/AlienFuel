using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Controle de Jogo")]
    [SerializeField] private int totalOrbsNaCena = 5;
    private int orbsColetados = 0;

    [Header("Interface (UI)")]
    [SerializeField] private TextMeshProUGUI textoPlacar;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        AtualizarTextoPlacar();
    }

    public void RegistrarColetaDeOrb()
    {
        orbsColetados++;
        Debug.Log("Orb coletado! Total: " + orbsColetados + "/" + totalOrbsNaCena);
        
        AtualizarTextoPlacar();

        if (orbsColetados >= totalOrbsNaCena)
        {
            VencerJogo();
        }
    }

    private void AtualizarTextoPlacar()
    {
        if (textoPlacar != null)
        {
            textoPlacar.text = "Orbs: " + orbsColetados + " / " + totalOrbsNaCena;
        }
    }

    private void VencerJogo()
    {
        Debug.Log("Parabéns! Você concluiu o objetivo do ambiente virtual!");
    }
}