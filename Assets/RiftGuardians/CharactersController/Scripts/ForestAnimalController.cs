using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ForestAnimalController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;
    public float jumpForce = 6f;
    public float airControl = 0.5f;

    [Header("Physics Settings")]
    public float gravityMultiplier = 2f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveInput;     // X y Y (de InputSystem)
    private bool isGrounded = false;
    private Vector3 groundNormal = Vector3.up;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // 🔹 Comprobar si está tocando el suelo
        GroundCheck();

        // 🔹 Movimiento plano (XZ)
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        // Mantener la velocidad actual para suavidad
        Vector3 velocity = rb.linearVelocity;
        Vector3 targetVelocity = moveDir * moveSpeed;
        targetVelocity.y = velocity.y;

        float control = isGrounded ? 1f : airControl;
        rb.linearVelocity = Vector3.Lerp(velocity, targetVelocity, control);

        // 🔹 Rotación hacia dirección de movimiento
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);
        }

        // 🔹 Aplicar gravedad extra para caída más natural
        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    // ----------------------------
    // Métodos de control externo
    // ----------------------------

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void PerformJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // ----------------------------
    // Detección de suelo
    // ----------------------------

    private void GroundCheck()
    {
        // Raycast vertical para detectar suelo
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
                            Vector3.down,
                            out RaycastHit hit,
                            groundCheckDistance,
                            groundLayer))
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
        }

        // Opcional: dibujar el rayo en escena
        Debug.DrawRay(transform.position + Vector3.up * 0.1f,
                      Vector3.down * groundCheckDistance,
                      isGrounded ? Color.green : Color.red);
    }
}
