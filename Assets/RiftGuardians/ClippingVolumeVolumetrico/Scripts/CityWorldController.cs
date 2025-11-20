using UnityEngine;

public class CityWorldController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;

    [Header("Movement Bounds (XZ)")]
    public Vector2 minBounds = new Vector2(-12f, -12f);
    public Vector2 maxBounds = new Vector2(12f, 12f);

    public bool IsAtBorder { get; private set; }

    [Header("Zoom Settings")]
    public float zoomSpeed = 4f;
    public float minScale = 0.5f;
    public float maxScale = 3f;

    [Header("Clipping Volume Pivot")]
    public Transform clippingPivot;
    // ← Debe ser el objeto que representa el centro de tu volumen de recorte

    private Vector2 moveInput;
    private float targetZoom = 1f;
    private float currentZoom = 1f;
    public float ZoomVelocity { get; private set; }
    public Vector2 MoveInput => moveInput;


    void Start()
    {
        currentZoom = transform.localScale.x;
        targetZoom = currentZoom;
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    // ----------------------------------------------------
    // MOVEMENT (solo XZ, sin rotación)
    // ----------------------------------------------------
    void HandleMovement()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Vector3 delta = moveDir.normalized * moveSpeed * Time.deltaTime;
            Vector3 newPos = transform.position + delta;

            // CLAMP de movimiento
            float clampedX = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
            float clampedZ = Mathf.Clamp(newPos.z, minBounds.y, maxBounds.y);

            // Detecta borde
            IsAtBorder = (clampedX != newPos.x) || (clampedZ != newPos.z);

            transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
        }
        else
        {
            IsAtBorder = false;
        }
    }


    // ----------------------------------------------------
    // ZOOM — desde el pivot del clipping volume
    // ----------------------------------------------------
    void HandleZoom()
    {
        float prevZoom = currentZoom;
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSpeed);
        transform.localScale = Vector3.one * currentZoom;

        ZoomVelocity = currentZoom - prevZoom; // valor positivo o negativo

        // Si no hay pivot, simplemente escala
        if (clippingPivot == null)
        {
            transform.localScale = Vector3.one * currentZoom;
            return;
        }

        // -------- Pivot-based scaling --------
        Vector3 pivot = clippingPivot.position;
        Vector3 direction = (transform.position - pivot);

        float scaleRatio = currentZoom / Mathf.Max(prevZoom, 0.0001f);

        // Reposicionar el mundo para que la escala salga desde el pivot
        transform.position = pivot + direction * scaleRatio;

        // Aplicar la escala uniforme
        transform.localScale = Vector3.one * currentZoom;
    }

    // ----------------------------------------------------
    // EXTERNAL INPUT METHODS
    // ----------------------------------------------------
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void AddZoom(float amount)
    {
        targetZoom += amount;
        targetZoom = Mathf.Clamp(targetZoom, minScale, maxScale);
    }

    public void SetZoom(float normalized01)
    {
        targetZoom = Mathf.Lerp(minScale, maxScale, normalized01);
    }
}
