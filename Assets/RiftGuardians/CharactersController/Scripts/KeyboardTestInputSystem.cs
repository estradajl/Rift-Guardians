using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardTestInputSystem : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActionsAsset;
    private InputAction moveAction;
    private InputAction jumpAction;

    [Header("Controllers")]
    public ForestAnimalController forestAnimal;
    public OceanFishController fishController;

    [Header("Mode Control")]
    public bool isUnderwater = false; // false = terrestre, true = acuático
    public Transform cameraTransform; // opcional, para dirección de cámara

    [Header("Debug")]
    public bool showDebug = true;

    private Vector2 moveInput2D = Vector2.zero;
    private Vector3 moveInput3D = Vector3.zero;

    void OnEnable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player", true);
        moveAction = playerMap.FindAction("Move", true);
        jumpAction = playerMap.FindAction("Jump", true);

        moveAction.Enable();
        jumpAction.Enable();

        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        jumpAction.performed += OnJumpPerformed;
    }

    void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        jumpAction.performed -= OnJumpPerformed;

        moveAction.Disable();
        jumpAction.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput2D = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput2D = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (isUnderwater && fishController != null)
        {
            fishController.PerformDash();
        }
        else if (!isUnderwater && forestAnimal != null)
        {
            forestAnimal.PerformJump();
        }
    }

    void FixedUpdate()
    {
        // 🔄 Convertir el Vector2 del input a Vector3 para movimiento 3D
        moveInput3D = new Vector3(moveInput2D.x, 0f, moveInput2D.y);

        // Si hay cámara, orienta el movimiento según su rotación (muy útil en 3D/MR)
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0;
            camRight.Normalize();

            moveInput3D = (camForward * moveInput2D.y + camRight * moveInput2D.x);
        }

        // 🔹 Enviar movimiento al controlador activo
        if (isUnderwater && fishController != null)
        {
            // El pez usa Vector2 (X=horizontal, Y=vertical)
            fishController.SetMoveInput(moveInput2D);
        }
        else if (!isUnderwater && forestAnimal != null)
        {
            // El animal usa Vector2 (X,Z)
            forestAnimal.SetMoveInput(new Vector2(moveInput3D.x, moveInput3D.z));
        }
    }

    void Update()
    {
        // Alternar modo (Animal / Pez)
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            isUnderwater = !isUnderwater;
    }

    void OnGUI()
    {
        if (!showDebug) return;
        string mode = isUnderwater ? "🐟 Underwater (Fish)" : "🦊 Ground (Animal)";
        GUI.Label(new Rect(10, 10, 400, 25), $"Mode: {mode}");
        GUI.Label(new Rect(10, 30, 400, 25), $"Move Input (2D): {moveInput2D}");
        GUI.Label(new Rect(10, 50, 400, 25), $"Move Input (3D): {moveInput3D}");
        GUI.Label(new Rect(10, 70, 400, 25), $"Jump/Dash: SPACE | Switch Mode: TAB");
    }
}
