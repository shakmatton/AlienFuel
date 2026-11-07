using UnityEngine;

public class EndingPanelController : MonoBehaviour
{
    [Header("Painel de encerramento")]
    [SerializeField] private GameObject painelFinal;

    private bool encerramentoExibido = false;


    private void Awake()
    {
        if (painelFinal == null)
        {
            Debug.LogError(
                "EndingPanelController: configure o painel final.",
                this
            );

            return;
        }

        // Começa com o painel escondido.
        painelFinal.SetActive(false);
    }


    // Exibe o painel sem modificar seus textos.
    public void ExibirEncerramentoAgora()
    {
        if (encerramentoExibido || painelFinal == null)
            return;

        encerramentoExibido = true;

        painelFinal.SetActive(true);

        Debug.Log("Painel de encerramento exibido!");
    }
}