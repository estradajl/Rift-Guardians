using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardTestInputSystemCity : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActionsAsset;

    private InputAction moveAction;
    private InputAction previousAction;   // Zoom Out
    private InputAction nextAction;       // Zoom In

    [Header("Controller")]
    public CityWorldController cityController;

    [Header("Camera")]
    public Transform cameraTransform; // opcional (para orientar movimiento)

    [Header("Zoom Settings")]
    public float zoomStep = 0.15f;    // cuanto zoom por pulsación

    [Header("Debug")]
    public bool showDebug = true;

    private Vector2 move2D = Vector2.zero;
    private Vector3 move3D = Vector3.zero;

    void OnEnable()
    {
        var playerMap = inputActionsAsset.FindActionMap("Player", true);

        moveAction = playerMap.FindAction("Move", true);
        previousAction = playerMap.FindAction("Previous", true); // Zoom OUT
        nextAction = playerMap.FindAction("Next", true);     // Zoom IN

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
    // MOVIMIENTO
    // -------------------------------

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        move2D = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        move2D = Vector2.zero;
    }

    // -------------------------------
    // ZOOM
    // -------------------------------

    private void OnZoomOut(InputAction.CallbackContext ctx)
    {
        if (cityController != null)
            cityController.AddZoom(-zoomStep);
    }

    private void OnZoomIn(InputAction.CallbackContext ctx)
    {
        if (cityController != null)
            cityController.AddZoom(+zoomStep);
    }

    // -------------------------------

    void FixedUpdate()
    {
        // Convertir a 3D
        move3D = new Vector3(move2D.x, 0f, move2D.y);

        // Orientar con la cámara
        if (cameraTransform != null)
        {
            Vector3 camF = cameraTransform.forward;
            camF.y = 0;
            camF.Normalize();

            Vector3 camR = cameraTransform.right;
            camR.y = 0;
            camR.Normalize();

            move3D = camF * move2D.y + camR * move2D.x;
        }

        // Enviar movimiento al CityWorldController
        if (cityController != null)
        {
            cityController.SetMoveInput(new Vector2(move3D.x, move3D.z));
        }
    }

    // -------------------------------
    // DEBUG
    // -------------------------------

    void OnGUI()
    {
        if (!showDebug) return;

        GUI.Label(new Rect(10, 10, 400, 25), $"Move 2D: {move2D}");
        GUI.Label(new Rect(10, 30, 400, 25), $"Move 3D: {move3D}");
        GUI.Label(new Rect(10, 50, 400, 25), $"Zoom: {cityController?.transform.localScale.x:F2}");
    }
}
