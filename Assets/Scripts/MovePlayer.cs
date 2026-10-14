using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer : MonoBehaviour
{
    private CharacterController ch;

    private float movementX;
    private float movementY;

    public float speed = 1.5f;
    private float tempSpeed;

    void Start()
    {
        tempSpeed = speed;
        ch = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 movementInput = ctx.ReadValue<Vector2>();
        movementX = movementInput.x;
        movementY = movementInput.y;
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) tempSpeed = speed * 2f;
        else if (ctx.canceled) tempSpeed = speed;
    }

    void Update()
    {
        // Pega o input e transforma em movimento relativo para onde o Player está olhando
        Vector3 moveDir = transform.right * movementX + transform.forward * movementY;

        // Move o CharacterController
        ch.Move(moveDir * tempSpeed * Time.deltaTime);

        // Adiciona uma gravidade simples para garantir que ele fique no chão
        if (!ch.isGrounded)
        {
            ch.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }
}