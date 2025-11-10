using UnityEngine;

public class MicroGestureInputManager : MonoBehaviour
{
    [Header("References")]
    public OVRHand leftHand;
    public OVRHand rightHand;
    public ForestAnimalController animalController;
    public OceanFishController fishController;

    [Header("Settings")]
    public bool useRightHand = true;
    public bool isUnderwater = false; // false = animal, true = fish
    public float inputCooldown = 0.2f;  // anti-spam para gestos rápidos
    public float smoothDecaySpeed = 5f; // velocidad del suavizado al soltar el gesto

    private OVRHand.MicrogestureType currentGesture = OVRHand.MicrogestureType.Invalid;
    private Vector2 moveInput = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;
    private float lastInputTime;

    void Update()
    {
        var activeHand = useRightHand ? rightHand : leftHand;
        if (activeHand == null)
        {
            Debug.LogWarning("⚠️ Asigna leftHand/rightHand en el inspector.");
            return;
        }

        // Detectar el gesto actual
        var detectedGesture = activeHand.GetMicrogestureType();

        // Si cambió el gesto, actualiza el comportamiento
        if (detectedGesture != currentGesture)
        {
            currentGesture = detectedGesture;
            lastInputTime = Time.time;
            HandleGestureChange(currentGesture);
        }

        // Si no hay gesto activo, reducir gradualmente el input hacia 0
        if (currentGesture == OVRHand.MicrogestureType.Invalid ||
            currentGesture == (OVRHand.MicrogestureType)0)
        {
            targetInput = Vector2.zero;
        }

        // Aplicar interpolación suave para “soltar” el movimiento
        moveInput = Vector2.Lerp(moveInput, targetInput, Time.deltaTime * smoothDecaySpeed);

        // Enviar el input suavizado a los controladores
        SendInputToControllers();
    }

    private void HandleGestureChange(OVRHand.MicrogestureType gesture)
    {
        switch (gesture)
        {
            case OVRHand.MicrogestureType.SwipeLeft:
                targetInput = Vector2.left;
                break;

            case OVRHand.MicrogestureType.SwipeRight:
                targetInput = Vector2.right;
                break;

            case OVRHand.MicrogestureType.SwipeForward:
                targetInput = Vector2.up;
                break;

            case OVRHand.MicrogestureType.SwipeBackward:
                targetInput = Vector2.down;
                break;

            case OVRHand.MicrogestureType.ThumbTap:
                PerformDashOrJump();
                targetInput = Vector2.zero;
                break;

            default:
                targetInput = Vector2.zero;
                break;
        }
    }

    private void PerformDashOrJump()
    {
        if (isUnderwater && fishController != null)
            fishController.PerformDash();
        else if (!isUnderwater && animalController != null)
            animalController.PerformJump();
    }

    private void SendInputToControllers()
    {
        if (isUnderwater && fishController != null)
        {
            fishController.SetMoveInput(moveInput);
        }
        else if (!isUnderwater && animalController != null)
        {
            animalController.SetMoveInput(moveInput);
        }
    }

    // Cambiar entre modos (opcional)
    public void ToggleMode()
    {
        isUnderwater = !isUnderwater;
        Debug.Log($"🌊 Modo cambiado: {(isUnderwater ? "Fish (Underwater)" : "Animal (Ground)")}");
    }
}