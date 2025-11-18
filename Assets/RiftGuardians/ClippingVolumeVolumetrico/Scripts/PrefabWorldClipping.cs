using UnityEngine;

public class PrefabWorldClipping : MonoBehaviour
{
    [Header("Clip Volume")]
    public Transform volume;

    [Header("Renderers to Clip (Lit)")]
    public Renderer[] clippedRenderers;

    [Header("Renderers to Fill (Hologram)")]
    public Renderer[] fillerRenderers;

    [Header("Clip Settings")]
    public int clipMode = 0;

    // Internal cached values to detect changes
    private Vector3 lastCenter;
    private Vector3 lastSize;
    private int lastClipMode;

    MaterialPropertyBlock mpbClip;
    MaterialPropertyBlock mpbFill;

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
    }

    void Update()
    {
        if (!volume) return;

        // Compute current parameters
        Vector3 center = volume.position;
        Vector3 size = volume.lossyScale * 0.5f;

        // Detect changes
        if (center != lastCenter || size != lastSize || clipMode != lastClipMode)
        {
            ApplyClipping(center, size, clipMode);

            lastCenter = center;
            lastSize = size;
            lastClipMode = clipMode;
        }
    }

    void ApplyClipping(Vector3 center, Vector3 size, int mode)
    {
        // Update clipped meshes
        if (clippedRenderers != null)
        {
            foreach (var r in clippedRenderers)
            {
                if (!r) continue;

                r.GetPropertyBlock(mpbClip);
                mpbClip.SetVector("_ClipCenter", center);
                mpbClip.SetVector("_ClipSize", size);
                mpbClip.SetFloat("_ClipMode", mode);
                r.SetPropertyBlock(mpbClip);
            }
        }

        // Update filler meshes
        if (fillerRenderers != null)
        {
            foreach (var r in fillerRenderers)
            {
                if (!r) continue;

                r.GetPropertyBlock(mpbFill);
                mpbFill.SetVector("_ClipCenter", center);
                mpbFill.SetVector("_ClipSize", size);
                mpbFill.SetFloat("_ClipMode", mode);
                r.SetPropertyBlock(mpbFill);
            }
        }
    }
}
