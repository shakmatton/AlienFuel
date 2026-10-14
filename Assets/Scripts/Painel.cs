using UnityEngine;
using TMPro;

public class Painel : MonoBehaviour
{
    [Header("Referências de UI (TMPro)")]
    [SerializeField] private TextMeshProUGUI textoFeedback;

    // Método chamado pelo evento OnClick() do botão no Canvas World Space
    public void BotaoFoiClicado()
    {
        Debug.Log("Botão do World Space acionado!");

        if (textoFeedback != null)
        {
            textoFeedback.text = "Painel Ativado com Sucesso!";
        }
    }
}