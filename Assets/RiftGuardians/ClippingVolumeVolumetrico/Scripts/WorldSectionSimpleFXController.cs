using UnityEngine;

/// <summary>
/// Control centralizado de:
/// - Clipping volumétrico en tiempo real para modelos lit + filler.
/// - Modo de FX del filler (_FillerMode) sin tocar speed/density/strength.
///
/// Mantiene la lógica original de PrefabWorldClipping (center/size/mode)
/// y añade un control simple de FX listo para usar desde otros scripts.
/// </summary>
public class WorldSectionSimpleFXController : MonoBehaviour
{
    public enum FillerFXMode { Scan = 0, Pulse = 1, Shell = 2 }

    [Header("Clip Volume Transform")]
    public Transform volume;

    [Header("Lit Renderers (Clipped)")]
    public Renderer[] clippedRenderers;

    [Header("Filler Renderers (Hologram FX)")]
    public Renderer[] fillerRenderers;

    [Header("Clipping Mode (0=Sphere 1=Box 2=Hemisphere)")]
    [Range(0, 2)]
    public int clipMode = 0;

    // FX mode controlled from outside
    [HideInInspector]
    public FillerFXMode currentFXMode = FillerFXMode.Shell;

    // Cache states for optimization
    Vector3 lastCenter;
    Vector3 lastSize;
    int lastClipMode;
    FillerFXMode lastFXMode;

    MaterialPropertyBlock mpbClip, mpbFill;

    // Shader IDs
    static readonly int ClipCenterID = Shader.PropertyToID("_ClipCenter");
    static readonly int ClipSizeID = Shader.PropertyToID("_ClipSize");
    static readonly int ClipModeID = Shader.PropertyToID("_ClipMode");
    static readonly int FillerModeID = Shader.PropertyToID("_FillerMode");

    void Awake()
    {
        mpbClip = new MaterialPropertyBlock();
        mpbFill = new MaterialPropertyBlock();

        if (volume == null)
            volume = transform.Find("ClipVolume");

        if ((clippedRenderers == null || clippedRenderers.Length == 0) && transform.Find("ModelClipped"))
            clippedRenderers = transform.Find("ModelClipped").GetComponentsInChildren<Renderer>();

        if ((fillerRenderers == null || fillerRenderers.Length == 0) && transform.Find("ModelSectionFiller"))
            fillerRenderers = transform.Find("ModelSectionFiller").GetComponentsInChildren<Renderer>();

        lastCenter = Vector3.positiveInfinity;
        lastSize = Vector3.positiveInfinity;
        lastFXMode = (FillerFXMode)(-1);
    }

    // ------------------------------
    // MAIN UPDATE: clipping + FX
    // ------------------------------
    void Update()
    {
        if (!volume) return;

        UpdateClipping();
        UpdateFXMode();
    }

    // ------------------------------
    // CLIPPING
    // ------------------------------
    void UpdateClipping()
    {
        Vector3 center = volume.position;
        Vector3 size = volume.lossyScale * 0.5f;

        if (center == lastCenter && size == lastSize && clipMode == lastClipMode)
            return;

        lastCenter = center;
        lastSize = size;
        lastClipMode = clipMode;

        // Update Lit renderers
        foreach (var r in clippedRenderers)
        {
            if (!r) continue;
            r.GetPropertyBlock(mpbClip);
            mpbClip.SetVector(ClipCenterID, center);
            mpbClip.SetVector(ClipSizeID, size);
            mpbClip.SetFloat(ClipModeID, clipMode);
            r.SetPropertyBlock(mpbClip);
        }

        // Update filler renderers
        foreach (var r in fillerRenderers)
        {
            if (!r) continue;
            r.GetPropertyBlock(mpbFill);
            mpbFill.SetVector(ClipCenterID, center);
            mpbFill.SetVector(ClipSizeID, size);
            mpbFill.SetFloat(ClipModeID, clipMode);
            r.SetPropertyBlock(mpbFill);
        }
    }

    // ------------------------------
    // FILLER MODE
    // ------------------------------
    void UpdateFXMode()
    {
        if (currentFXMode == lastFXMode)
            return;

        lastFXMode = currentFXMode;

        foreach (var r in fillerRenderers)
        {
            if (!r) continue;
            r.GetPropertyBlock(mpbFill);
            mpbFill.SetFloat(FillerModeID, (float)currentFXMode);
            r.SetPropertyBlock(mpbFill);
        }
    }

    // ------------------------------
    // PUBLIC API
    // ------------------------------
    public void SetFX(FillerFXMode mode)
    {
        currentFXMode = mode;
    }
}
