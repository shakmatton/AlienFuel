using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float distanciaInteracao = 3f;

    [Header("Input System")]
    [SerializeField] private InputActionReference grabAction; // Arraste a Action 'Grab' aqui

    private void OnEnable()
    {
        if (grabAction != null) grabAction.action.Enable();
    }

    private void OnDisable()
    {
        if (grabAction != null) grabAction.action.Disable();
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distanciaInteracao))
        {
            // Procura o script Orb no objeto olhado
            Orb orbAlvo = hit.collider.GetComponent<Orb>();

            if (orbAlvo != null)
            {
                if (grabAction != null && grabAction.action.WasPressedThisFrame())
                {
                    orbAlvo.ExecutarInteracao();
                }
            }
        }
    }
}