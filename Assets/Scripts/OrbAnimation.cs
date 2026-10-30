using UnityEngine;

public class OrbAnimation : MonoBehaviour
{
    [Header("Rotação")]
    [SerializeField] private float velocidadeGiro = 6f;

    [Header("Flutuação")]
    [SerializeField] private float velocidadeFlutuacao = 1.5f;
    [SerializeField] private float alturaFlutuacao = 0.15f;

    private Vector3 posicaoInicialLocal;

    private bool rotacaoAtiva = true;
    private bool flutuacaoAtiva = true;


    private void Awake()
    {
        // Guarda a posição local original do objeto visual.
        posicaoInicialLocal = transform.localPosition;
    }


    private void Update()
    {
        // ROTATION
        if (rotacaoAtiva)
        {
            transform.Rotate(
                Vector3.up * velocidadeGiro * Time.deltaTime,
                Space.Self
            );
        }

        // FLOATING
        if (flutuacaoAtiva)
        {
            float novaY = posicaoInicialLocal.y +
                          Mathf.Sin(Time.time * velocidadeFlutuacao)
                          * alturaFlutuacao;

            transform.localPosition = new Vector3(
                posicaoInicialLocal.x,
                novaY,
                posicaoInicialLocal.z
            );
        }
    }


    // Permite ativar ou desativar a flutuação independentemente.
    public void DefinirFlutuacao(bool ativa)
    {
        flutuacaoAtiva = ativa;

        // Retorna à posição local original ao interromper a flutuação.
        if (!ativa)
        {
            transform.localPosition = posicaoInicialLocal;
        }
    }


    // Permite ativar ou desativar a rotação.
    public void DefinirRotacao(bool ativa)
    {
        rotacaoAtiva = ativa;
    }


    // Interrompe todas as animações.
    public void PararAnimacoes()
    {
        DefinirFlutuacao(false);
        DefinirRotacao(false);
    }
}