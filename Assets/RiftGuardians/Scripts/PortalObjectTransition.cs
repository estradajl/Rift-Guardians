using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Transform))]
public class PortalObjectTransition : MonoBehaviour
{
    [Header("FX Settings")]
    public float dissolveDuration = 1.5f;
    public Texture2D noiseTexture;
    public Color edgeColor = new Color(0.3f, 0.8f, 1f);
    public float edgeWidth = 0.05f;

    [Header("Layers")]
    public string defaultLayerName = "Default";
    public string portalLayerName = "PortalWorld";

    private Renderer[] renderers;   // ✅ Soportará múltiples renderers hijos
    private MaterialPropertyBlock mpb;
    private bool transitioning = false;

    void Awake()
    {
        // ✅ Busca todos los renderers en hijos (incluso desactivados)
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();
        foreach (var rend in renderers)
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetTexture("_NoiseMap", noiseTexture);
            mpb.SetColor("_EdgeColor", edgeColor);
            mpb.SetFloat("_EdgeWidth", edgeWidth);
            rend.SetPropertyBlock(mpb);
        }
    }

    public void TriggerTransition(bool enteringPortal)
    {
        if (!transitioning)
            StartCoroutine(DissolveRoutine(enteringPortal));
    }

    private IEnumerator DissolveRoutine(bool enteringPortal)
    {
        transitioning = true;
        float t = 0f;

        int startLayer = LayerMask.NameToLayer(enteringPortal ? portalLayerName : defaultLayerName);
        int endLayer = LayerMask.NameToLayer(enteringPortal ? defaultLayerName : portalLayerName);

        // ✅ Cambia layer de todo el objeto y sus hijos
        SetLayerRecursively(gameObject, startLayer);

        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float dissolveValue = enteringPortal ? t / dissolveDuration : 1 - (t / dissolveDuration);
            foreach (var rend in renderers)
            {
                rend.GetPropertyBlock(mpb);
                mpb.SetFloat("_DissolveAmount", dissolveValue);
                rend.SetPropertyBlock(mpb);
            }
            yield return null;
        }

        SetLayerRecursively(gameObject, endLayer);
        transitioning = false;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
