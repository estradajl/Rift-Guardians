using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardTestInputSystemCity : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActionsAsset;

    private InputAction moveAction;
    private InputAction previousAction;   // Zoom Out
    private InputAction nextAction;       // Zoom In

    [Header("Controllers")]
    public CityWorldController cityController;    // Solo mueve la ciudad

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Zoom Settings")]
    public float zoomStep = 0.15f;

    [Header("Debug")]
    public bool showDebug = true;
    public Color debugColor = Color.cyan;

    private Vector2 move2D = Vector2.zero;
    private Vector3 move3D = Vector3.zero;
    private float zoomInput = 0f;

    void OnEnable()
    {
        var map = inputActionsAsset.FindActionMap("Player", true);

        moveAction = map.FindAction("Move", true);
        previousAction = map.FindAction("Previous", true);
        nextAction = map.FindAction("Next", true);

        moveAction.Enable();
        previousAction.Enable();
        nextAction.Enable();

        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;

        previousAction.performed += OnZoomOut;
        nextAction.performed += OnZoomIn;
    }

    void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;

        previousAction.performed -= OnZoomOut;
        nextAction.performed -= OnZoomIn;

        moveAction.Disable();
        previousAction.Disable();
        nextAction.Disable();
    }

    // -------------------------------
    // MOVEMENT INPUT
    // -------------------------------
    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        move2D = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        move2D = Vector2.zero;
    }

    // -------------------------------
    // ZOOM INPUT
    // -------------------------------
    private void OnZoomOut(InputAction.CallbackContext ctx)
    {
        zoomInput = -zoomStep;
    }

    private void OnZoomIn(InputAction.CallbackContext ctx)
    {
        zoomInput = zoomStep;
    }

    // -------------------------------
    // MAIN UPDATE → REAL CITY MOVEMENT
    // -------------------------------
    void FixedUpdate()
    {
        if (!cityController) return;

        // Convert movement relative to camera
        move3D = ConvertMoveToWorld(move2D);

        bool isMoving = move3D.sqrMagnitude > 0.0001f;
        bool isZooming = Mathf.Abs(zoomInput) > 0.0001f;

        // --- Move City ---
        if (isMoving)
            cityController.SetMoveInput(new Vector2(move3D.x, move3D.z));
        else
            cityController.SetMoveInput(Vector2.zero);

        // --- Zoom City ---
        if (isZooming)
            cityController.AddZoom(zoomInput);

        // Zoom lasts only 1 frame
        zoomInput = 0;
    }

    // -------------------------------
    // Convert 2D input into world-space movement
    // -------------------------------
    Vector3 ConvertMoveToWorld(Vector2 input)
    {
        if (!cameraTransform)
            return new Vector3(input.x, 0, input.y);

        Vector3 f = cameraTransform.forward;
        f.y = 0;
        f.Normalize();

        Vector3 r = cameraTransform.right;
        r.y = 0;
        r.Normalize();

        return f * input.y + r * input.x;
    }

    // -------------------------------
    // DEBUG GUI
    // -------------------------------
    void OnGUI()
    {
        if (!showDebug) return;

        GUI.color = debugColor;

        GUI.Label(new Rect(10, 10, 600, 25), $"Move2D: {move2D}");
        GUI.Label(new Rect(10, 30, 600, 25), $"Move3D: {move3D}");
        GUI.Label(new Rect(10, 50, 600, 25), $"Zoom Frame: {zoomInput}");
        GUI.Label(new Rect(10, 70, 600, 25), $"ZoomVelocity: {cityController.ZoomVelocity:F4}");
    }
}
