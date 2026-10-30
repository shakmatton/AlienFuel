using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOrbInteraction : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Camera cameraJogador;
    [SerializeField] private Transform pontoDeApoio;
    [SerializeField] private Transform pontoDeInsercao;

    [Header("Input System")]
    [SerializeField] private InputActionReference acaoSegurarOrb;

    [Header("Interação")]
    [SerializeField] private float distanciaParaPegar = 3f;

    private Orb orbSegurado;


    private void OnEnable()
    {
        if (acaoSegurarOrb != null && acaoSegurarOrb.action != null)
        {
            acaoSegurarOrb.action.Enable();
        }
    }


    private void OnDisable()
    {
        if (acaoSegurarOrb != null && acaoSegurarOrb.action != null)
        {
            acaoSegurarOrb.action.Disable();
        }
    }

    
    private void Update()
    {
        if (acaoSegurarOrb == null ||
            acaoSegurarOrb.action == null)
            return;

        // Cada clique alterna entre pegar e soltar.
        if (acaoSegurarOrb.action.WasPressedThisFrame())
        {
            if (orbSegurado == null)
            {
                TentarPegarOrb();
            }
            else
            {
                EnviarOrbParaNave();
            }
        }
    }


    private void TentarPegarOrb()
    {
        if (cameraJogador == null || pontoDeApoio == null)
        {
            Debug.LogError("PlayerOrbInteraction: configure a câmera e o ponto de apoio.", this);
            return;
        }

        // Cria um raio a partir do centro da câmera.
        Ray raio = new Ray(cameraJogador.transform.position, cameraJogador.transform.forward);

        // Verifica se há algum objeto atingido dentro do alcance.
        if (Physics.Raycast(raio, out RaycastHit hit, distanciaParaPegar))
        {
            // Procura o componente Orb no objeto atingido, ou em algum de seus objetos pais.
            Orb orbEncontrado = hit.collider.GetComponentInParent<Orb>();

            if (orbEncontrado != null && orbEncontrado.PodeSerPego)
            {
                // Tenta pegar o orb.
                if (orbEncontrado.Pegar(pontoDeApoio))
                {
                    orbSegurado = orbEncontrado;

                    Debug.Log("Orb pego: " + orbSegurado.name);
                }
            }
        }
    }


    private void EnviarOrbParaNave()
    {
        if (pontoDeInsercao == null)
        {
            Debug.LogError("PlayerOrbInteraction: configure o ponto de inserção da nave.", this);
            return;
        }

        // Solicita ao próprio orb que inicie o transporte.
        bool enviado = orbSegurado.EnviarParaNave(pontoDeInsercao, AoChegarAoDestino);

        // Libera a referência após iniciar o transporte.
        if (enviado)
        {
            Debug.Log("Orb enviado para a nave!");
            orbSegurado = null;
        }
    }


    // Executado quando o orb chega ao destino.
    // Posteriormente, o ShipFunnel assumirá o processamento da entrega.
    private void AoChegarAoDestino(Orb orb)
    {
        Debug.Log("Orb chegou à entrada da nave: " + orb.name);
    }
}
