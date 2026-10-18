using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody; // Arraste o Player aqui no Inspector

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
        float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;

        // Rotação vertical (olhar para cima e para baixo) restrita aos limites que você pediu
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 25f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotação horizontal (olhar para os lados) girando o corpo do Player diretamente no eixo Y
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}