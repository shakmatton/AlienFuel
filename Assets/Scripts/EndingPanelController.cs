using System.Collections;
using UnityEngine;

public class EndingPanelController : MonoBehaviour
{
    [Header("Painel Final")]
    [SerializeField] private GameObject painelFinal;

    [Header("Tempo de Espera")]
    [SerializeField] private float tempoAteEncerramento = 10f;

    private bool encerramentoIniciado;

    private void Start()
    {
        // Mantém o painel final escondido no início.
        painelFinal.SetActive(false);

        // Captura o mouse assim que a cena do jogo começa.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void IniciarEncerramento()
    {
        if (encerramentoIniciado)
            return;

        encerramentoIniciado = true;
        StartCoroutine(AguardarEncerramento());
    }

    private IEnumerator AguardarEncerramento()
    {
        yield return new WaitForSeconds(tempoAteEncerramento);

        // Exibe o painel final.
        painelFinal.SetActive(true);
    }
}