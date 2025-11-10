using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

[RequireComponent(typeof(Collider))]
public class PortalCrossable : MonoBehaviour
{
    [Header("Transition Settings")]
    public Renderer targetRenderer;
    public string dissolveProperty = "_DissolveAmount";
    public float transitionDuration = 1f;
    public int beforeLayer = 0;
    public int afterLayer = 8; // ejemplo: "VirtualObjects"

    [Header("References")]
    public Rigidbody rb;
    public HandGrabInteractable handGrab;

    [Header("Events")]
    public UnityEvent OnPortalEnter;
    public UnityEvent OnPortalExit;
    public UnityEvent OnTransitionComplete;

    private Material matInstance;
    private bool isCrossing;

    void Start()
    {
        if (targetRenderer)
            matInstance = targetRenderer.material;

        if (!rb)
            rb = GetComponent<Rigidbody>();

        if (!handGrab)
            handGrab = GetComponentInChildren<HandGrabInteractable>(true);

        if (handGrab) handGrab.enabled = false;
    }

    public void BeginPortalTransition(PortalZone portal)
    {
        if (isCrossing) return;
        StartCoroutine(PortalTransitionCoroutine(portal));
    }

    private IEnumerator PortalTransitionCoroutine(PortalZone portal)
    {
        isCrossing = true;
        float t = 0f;

        OnPortalEnter?.Invoke();

        // 🔹 1. Dissolve hacia desaparecer
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float dissolve = Mathf.SmoothStep(0, 1, t / transitionDuration);
            if (matInstance)
                matInstance.SetFloat(dissolveProperty, dissolve);
            yield return null;
        }

        // 🔹 2. Cambiar de layer para todo el objeto y sus hijos
        SetLayerRecursively(gameObject, afterLayer);

        // 🔹 3. Teletransportar al punto de salida
        if (rb) rb.isKinematic = true;
        transform.position = portal.GetExitPosition(transform.position);

        OnPortalExit?.Invoke();

        // 🔹 4. Dissolve inverso (reaparecer)
        t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float dissolve = Mathf.SmoothStep(1, 0, t / transitionDuration);
            if (matInstance)
                matInstance.SetFloat(dissolveProperty, dissolve);
            yield return null;
        }

        // 🔹 5. Reactivar físicas y activar modo de agarre
        if (rb) rb.isKinematic = false;
        EnableGrabMode();

        isCrossing = false;
        OnTransitionComplete?.Invoke();
    }

    private void EnableGrabMode()
    {
        if (handGrab) handGrab.enabled = true;

        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // ========================================================
    // 🔁 UTILIDAD RECURSIVA: Cambia la layer de todos los hijos
    // ========================================================
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
