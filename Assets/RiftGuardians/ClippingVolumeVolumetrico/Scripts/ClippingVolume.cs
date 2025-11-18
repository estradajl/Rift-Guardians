using UnityEngine;

[ExecuteAlways]
public class ClippingVolume : MonoBehaviour
{
    public enum ClipShape { Box = 0, Sphere = 1 }

    [Header("Volume Shape")]
    public ClipShape shape = ClipShape.Box;
    public Vector3 localSize = Vector3.one; // tamaño base (se multiplica por lossyScale)

    [Header("Targets")]
    public Renderer[] targetRenderers;

    [Header("Shader Property Names")]
    public string clipCenterProperty = "_ClipCenter";
    public string clipSizeProperty = "_ClipSize";
    public string clipShapeProperty = "_ClipShape";

    private MaterialPropertyBlock mpb;

    void OnEnable()
    {
        if (mpb == null)
            mpb = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            return;

        Vector3 center = transform.position;
        Vector3 worldSize = Vector3.Scale(localSize, transform.lossyScale);
        float shapeVal = (shape == ClipShape.Box) ? 0f : 1f;

        foreach (var r in targetRenderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetVector(clipCenterProperty, center);
            mpb.SetVector(clipSizeProperty, worldSize);
            mpb.SetFloat(clipShapeProperty, shapeVal);
            r.SetPropertyBlock(mpb);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation,
            Vector3.Scale(localSize, transform.lossyScale));

        Gizmos.matrix = m;

        if (shape == ClipShape.Box)
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        else
            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
    }
}
