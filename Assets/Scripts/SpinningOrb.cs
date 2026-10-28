using UnityEngine;

public class SpinningOrb : MonoBehaviour
{
    [Header("Configuração de Rotação")]
    public float velocidadeGiro = 6f; // Velocidade de rotação no eixo vertical (Y)

    void Update()
    {
        // Gira o objeto continuamente em torno do próprio eixo Y
        transform.Rotate(Vector3.up * velocidadeGiro * Time.deltaTime);
    }
}