using UnityEngine;

public class MicroGestureCityInputManager : MonoBehaviour
{
    [Header("Hand References")]
    public OVRHand leftHand;
    public OVRHand rightHand;

    [Header("Runtime City Binding")]
    public Transform cityRoot;               // referencia al objeto City instanciado
    public string cityRootName = "City(Clone)";
    public string modelClippedName = "ModelClipped";

    private CityWorldController cityController;
    private float searchInterval = 0.4f;
    private float nextSearchTime = 0f;

    [Header("Microgesture Settings")]
    public float smoothDecaySpeed = 5f;
    public float zoomStep = 0.15f;
    public float inputCooldown = 0.25f;

    private Vector2 targetMoveInput = Vector2.zero;
    private Vector2 smoothMoveInput = Vector2.zero;

    private OVRHand.MicrogestureType leftGesture = OVRHand.MicrogestureType.Invalid;
    private OVRHand.MicrogestureType rightGesture = OVRHand.MicrogestureType.Invalid;

    private float lastLeftInputTime = 0f;
    private float lastRightInputTime = 0f;

    // ----------------------------------------------------
    // UPDATE
    // ----------------------------------------------------
    void Update()
    {
        AutoBindCity();

        if (cityController == null)
            return;

        HandleLeftHandMovement();
        HandleRightHandZoom();

        smoothMoveInput = Vector2.Lerp(
            smoothMoveInput,
            targetMoveInput,
            Time.deltaTime * smoothDecaySpeed
        );

        cityController.SetMoveInput(smoothMoveInput);
    }

    // ----------------------------------------------------
    // AUTO-BIND A LA CIUDAD Y A ModelClipped
    // ----------------------------------------------------
    void AutoBindCity()
    {
        // Si ya tenemos controller → validar que siga existiendo
        if (cityController != null)
        {
            if (cityController.gameObject == null)
            {
                cityController = null;
                cityRoot = null;
            }
            else
            {
                return; // ya tenemos referencia operativa
            }
        }

        // Frecuencia de búsqueda controlada
        if (Time.time < nextSearchTime)
            return;

        nextSearchTime = Time.time + searchInterval;

        // 1) Si no tengo ciudadRoot → buscarlo
        if (cityRoot == null)
        {
            GameObject found = GameObject.Find(cityRootName);
            if (found != null)
            {
                cityRoot = found.transform;
                Debug.Log($"🏙 City Root encontrado: {found.name}");
            }
            else
            {
                return; // no hay ciudad aún
            }
        }

        // 2) Buscar ModelClipped interno
        Transform modelClipped = cityRoot.Find(modelClippedName);
        if (modelClipped == null)
        {
            Debug.LogWarning("⚠ No se encontró ModelClipped dentro de City.");
            return;
        }

        // 3) Obtener el CityWorldController dentro de ModelClipped
        cityController = modelClipped.GetComponentInChildren<CityWorldController>();

        if (cityController != null)
            Debug.Log("🌆 CityWorldController enlazado correctamente.");
    }

    // ----------------------------------------------------
    // MOVIMIENTO MANO IZQUIERDA
    // ----------------------------------------------------
    void HandleLeftHandMovement()
    {
        if (leftHand == null) return;

        var detected = leftHand.GetMicrogestureType();
        if (detected != leftGesture &&
            Time.time - lastLeftInputTime > inputCooldown)
        {
            leftGesture = detected;
            lastLeftInputTime = Time.time;
            ApplyLeftGestureMove(detected);
        }

        // Si no hay gesto, suaviza a cero
        if (detected == OVRHand.MicrogestureType.Invalid ||
            detected == (OVRHand.MicrogestureType)0)
        {
            targetMoveInput = Vector2.zero;
        }
    }

    void ApplyLeftGestureMove(OVRHand.MicrogestureType g)
    {
        switch (g)
        {
            case OVRHand.MicrogestureType.SwipeLeft:
                targetMoveInput = Vector2.left;
                break;
            case OVRHand.MicrogestureType.SwipeRight:
                targetMoveInput = Vector2.right;
                break;
            case OVRHand.MicrogestureType.SwipeForward:
                targetMoveInput = Vector2.up;
                break;
            case OVRHand.MicrogestureType.SwipeBackward:
                targetMoveInput = Vector2.down;
                break;
            default:
                targetMoveInput = Vector2.zero;
                break;
        }
    }

    // ----------------------------------------------------
    // ZOOM MANO DERECHA
    // ----------------------------------------------------
    void HandleRightHandZoom()
    {
        if (rightHand == null) return;

        var detected = rightHand.GetMicrogestureType();
        if (detected != rightGesture &&
            Time.time - lastRightInputTime > inputCooldown)
        {
            rightGesture = detected;
            lastRightInputTime = Time.time;
            ApplyRightGestureZoom(detected);
        }
    }

    void ApplyRightGestureZoom(OVRHand.MicrogestureType g)
    {
        switch (g)
        {
            case OVRHand.MicrogestureType.SwipeLeft:
                cityController.AddZoom(+zoomStep); // zoom in
                break;

            case OVRHand.MicrogestureType.SwipeRight:
                cityController.AddZoom(-zoomStep); // zoom out
                break;
        }
    }

    // ----------------------------------------------------
    // LIMPIEZA
    // ----------------------------------------------------
    void OnDestroy()
    {
        cityRoot = null;
        cityController = null;
        leftHand = null;
        rightHand = null;
    }
}
