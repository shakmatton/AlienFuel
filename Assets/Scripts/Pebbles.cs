using UnityEngine;

public class Pebbles : MonoBehaviour
{
    public float velocidade = 1.5f;
    public float altura = 0.15f;

    private Vector3 posicaoInicial;

    void Start()
    {
        posicaoInicial = transform.position;
    }

    void Update()
    {
        // Cria um movimento suave usando onda senoidal.
        float novaY = posicaoInicial.y +
                      Mathf.Sin(Time.time * velocidade) * altura;

        transform.position = new Vector3(
            posicaoInicial.x,
            novaY,
            posicaoInicial.z
        );
    }
}