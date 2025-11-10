using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class OceanFishController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float turnSpeed = 4f;           // velocidad angular (rad/s)
    public float dashForce = 8f;
    public float waterDrag = 2f;           // resistencia del agua
    public float verticalSpeed = 2f;       // subir/bajar en el eje Y

    [Header("Physics Settings")]
    public float maxTurnAnglePerFrame = 45f; // evita sobrepasar giros bruscos
    public float stabilizationTorque = 2f;   // fuerza para mantener orientación estable

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = waterDrag;
        rb.angularDamping = 1f;
        rb.constraints = RigidbodyConstraints.None;
    }

    void FixedUpdate()
    {
        MoveFish();
        StabilizeRotation();
    }

    // ===============================
    // External input interface
    // ===============================

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void PerformDash()
    {
        if (!isDashing)
            StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        rb.AddForce(transform.forward * dashForce, ForceMode.VelocityChange);
        yield return new WaitForSeconds(0.5f);
        isDashing = false;
    }

    // ===============================
    // Core movement and rotation logic
    // ===============================

    private void MoveFish()
    {
        // 1️⃣ Calcular dirección deseada (horizontal + vertical)
        Vector3 inputDir = new Vector3(moveInput.x, moveInput.y, 1f).normalized;

        // 2️⃣ Transformar al espacio local del pez
        Vector3 targetDirection = transform.TransformDirection(inputDir);

        // 3️⃣ Suavizar rotación (solo yaw y pitch, sin roll excesivo)
        Quaternion desiredRot = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desiredRot,
            maxTurnAnglePerFrame * Time.fixedDeltaTime * turnSpeed
        );

        // 4️⃣ Aplicar fuerza hacia adelante (con arrastre)
        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            transform.forward * moveSpeed,
            Time.fixedDeltaTime * turnSpeed
        );
    }

    private void StabilizeRotation()
    {
        // Obtener la dirección actual hacia adelante y arriba
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;

        // Recalcular el "up" deseado global (mantener el pez estable sobre Z)
        Vector3 desiredUp = Vector3.up;

        // Recalcular un right ortogonalizado a partir de forward y desiredUp
        Vector3 right = Vector3.Cross(desiredUp, forward).normalized;

        // Si el forward y el desiredUp son casi paralelos (cuando mira hacia arriba/abajo),
        // prevenimos jitter ajustando un fallback.
        if (right.sqrMagnitude < 0.001f)
            right = transform.right;

        // Recalcular el nuevo forward ortogonal
        forward = Vector3.Cross(right, desiredUp).normalized;

        // Calcular la nueva rotación estable
        Quaternion targetRotation = Quaternion.LookRotation(forward, desiredUp);

        // Suavizar hacia la nueva orientación sin vibración
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.fixedDeltaTime * stabilizationTorque
        );
    }
}