
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerOrbInteraction : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Camera cameraJogador;
    [SerializeField] private Transform pontoDeApoio;
    [SerializeField] private Transform pontoDeInsercao;
    [SerializeField] private OrbReleaseZone zonaDeEntrega;

    [Header("Interface")]
    [SerializeField] private TextMeshProUGUI textoAcao;
    [SerializeField] private GameObject miraCentral;

    [Header("Input System")]
    [SerializeField] private InputActionReference acaoSegurarOrb;

    [Header("Interação")]
    [SerializeField] private float distanciaParaPegar = 3f;

    private Orb orbSegurado;


    private void Awake()
    {
        OcultarAviso();
    }


    private void OnEnable()
    {
        if (acaoSegurarOrb != null &&
            acaoSegurarOrb.action != null)
        {
            acaoSegurarOrb.action.Enable();
        }
    }


    private void OnDisable()
    {
        if (acaoSegurarOrb != null &&
            acaoSegurarOrb.action != null)
        {
            acaoSegurarOrb.action.Disable();
        }

        OcultarAviso();
    }


    private void Update()
    {
        // Atualiza continuamente o aviso na tela.
        AtualizarAviso();

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
            else if (zonaDeEntrega != null &&
                     zonaDeEntrega.JogadorDentro)
            {
                EnviarOrbParaNave();
            }
        }
    }


    private void AtualizarAviso()
    {
        if (textoAcao == null)
            return;

        // SITUAÇÃO 1: jogador já está segurando um orb.
        if (orbSegurado != null)
        {
            // Só mostra "Dispare!" dentro da zona de entrega.
            if (zonaDeEntrega != null &&
                zonaDeEntrega.JogadorDentro)
            {
                MostrarAviso("Dispare!");
            }
            else
            {
                OcultarAviso();
            }

            return;
        }

        // SITUAÇÃO 2: jogador ainda não está segurando nada.
        // Verifica se está mirando em um orb próximo.
        Orb orbMirado = DetectarOrbMirado();

        if (orbMirado != null && orbMirado.PodeSerPego)
        {
            MostrarAviso("Colete!");
        }
        else
        {
            OcultarAviso();
        }
    }


    private Orb DetectarOrbMirado()
    {
        if (cameraJogador == null)
            return null;

        Ray raio = new Ray(
            cameraJogador.transform.position,
            cameraJogador.transform.forward
        );

        if (Physics.Raycast(
            raio,
            out RaycastHit hit,
            distanciaParaPegar))
        {
            // Procura o Orb no objeto atingido
            // ou em algum dos seus objetos pais.
            return hit.collider.GetComponentInParent<Orb>();
        }

        return null;
    }


    private void MostrarAviso(string mensagem)
    {
        if (textoAcao == null)
            return;

        textoAcao.text = mensagem;

        if (!textoAcao.gameObject.activeSelf)
        {
            textoAcao.gameObject.SetActive(true);
        }
    }


    private void OcultarAviso()
    {
        if (textoAcao != null &&
            textoAcao.gameObject.activeSelf)
        {
            textoAcao.gameObject.SetActive(false);
        }
    }


    private void TentarPegarOrb()
    {
        if (cameraJogador == null ||
            pontoDeApoio == null)
        {
            Debug.LogError(
                "PlayerOrbInteraction: configure a câmera e o ponto de apoio.",
                this
            );

            return;
        }

        Orb orbEncontrado = DetectarOrbMirado();

        if (orbEncontrado != null &&
            orbEncontrado.PodeSerPego)
        {
            if (orbEncontrado.Pegar(pontoDeApoio))
            {
                orbSegurado = orbEncontrado;

                Debug.Log("Orb pego: " + orbSegurado.name);
            }
        }
    }


    private void EnviarOrbParaNave()
    {
        if (pontoDeInsercao == null)
        {
            Debug.LogError(
                "PlayerOrbInteraction: configure o ponto de inserção da nave.",
                this
            );

            return;
        }

        bool enviado = orbSegurado.EnviarParaNave(
            pontoDeInsercao,
            AoChegarAoDestino
        );

        if (enviado)
        {
            Debug.Log("Orb enviado para a nave!");

            orbSegurado = null;
        }
    }


    private void AoChegarAoDestino(Orb orb)
    {
        Debug.Log(
            "Orb chegou à entrada da nave: " + orb.name
        );
    }
    
    private void LateUpdate()
    {
        if (miraCentral != null)
        {
            // A mira desaparece enquanto o jogador segura um orb.
            miraCentral.SetActive(orbSegurado == null);
        }
    }
}
