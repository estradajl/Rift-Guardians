using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PortalZone : MonoBehaviour
{
    [Header("Portal FX Settings")]
    [Tooltip("El Renderer con el material PortalSurfaceFX")]
    public Renderer portalSurfaceRenderer;

    [Range(0f, 0.3f)] public float maxWaveStrength = 0.1f;
    public float fadeSpeed = 1.5f;

    private Material portalMat;
    private Coroutine waveRoutine;

    private void Start()
    {
        if (portalSurfaceRenderer != null)
        {
            portalMat = portalSurfaceRenderer.material;
            portalMat.SetFloat("_WaveStrength", 0f);
            portalSurfaceRenderer.enabled = false; // empieza apagado
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var crossable = other.GetComponentInParent<PortalCrossable>();
        if (crossable == null) return;

        // activar portal visual
        if (portalSurfaceRenderer)
            portalSurfaceRenderer.enabled = true;

        if (waveRoutine != null)
            StopCoroutine(waveRoutine);

        waveRoutine = StartCoroutine(AnimateWave(0f, maxWaveStrength, fadeSpeed));

        // iniciar la transición del objeto
        crossable.BeginPortalTransition(this);
    }

    private void OnTriggerExit(Collider other)
    {
        var crossable = other.GetComponentInParent<PortalCrossable>();
        if (crossable == null) return;

        if (waveRoutine != null)
            StopCoroutine(waveRoutine);

        waveRoutine = StartCoroutine(AnimateWave(maxWaveStrength, 0f, fadeSpeed, () =>
        {
            // desactivar visual del portal al terminar el fade out
            if (portalSurfaceRenderer)
                portalSurfaceRenderer.enabled = false;
        }));
    }

    private IEnumerator AnimateWave(float from, float to, float speed, System.Action onFinish = null)
    {
        if (portalMat == null) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float value = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            portalMat.SetFloat("_WaveStrength", value);
            yield return null;
        }

        portalMat.SetFloat("_WaveStrength", to);
        onFinish?.Invoke();
    }

    // Este método lo usas en PortalCrossable.cs
    public Vector3 GetExitPosition(Vector3 currentPosition)
    {
        // Puedes personalizar este cálculo según tu escena
        // Por ejemplo, simplemente devuelve el otro lado del portal
        return transform.TransformPoint(Vector3.forward * 0.5f);
    }
}
