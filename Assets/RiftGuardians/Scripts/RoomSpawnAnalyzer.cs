using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;

[DefaultExecutionOrder(-10)]
public class RoomSpawnAnalyzer : MonoBehaviour
{
    public MRUKRoom CurrentRoom { get; private set; }

    [Header("Debug Settings")]
    public bool showDebug = true;
    public Color centerColor = Color.cyan;
    public Color ceilingColor = Color.yellow;
    public Color wallColor = Color.magenta;
    public float gizmoSize = 0.15f;

    private void Awake()
    {
        if (MRUK.Instance)
            MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    private void OnSceneLoaded()
    {
        CurrentRoom = MRUK.Instance.GetCurrentRoom();
    }

    // ============================================================
    // 🔹 ROOM CENTER
    // ============================================================
    public Vector3 GetRoomCenter()
    {
        if (CurrentRoom == null)
            return Vector3.zero;

        // ✅ v81: método directo sin "out"
        Bounds bounds = CurrentRoom.GetRoomBounds();
        return CurrentRoom.transform.TransformPoint(bounds.center);
    }

    // ============================================================
    // 🔹 CEILING CENTER
    // ============================================================
    public Vector3 GetCeilingCenter()
    {
        if (CurrentRoom == null)
            return GetRoomCenter() + Vector3.up * 2f;

        var ceilingAnchors = CurrentRoom.Anchors
            .Where(a => a.Label == MRUKAnchor.SceneLabels.CEILING)
            .ToList();

        if (ceilingAnchors.Count == 0)
            return GetRoomCenter() + Vector3.up * 2f;

        Vector3 avgPos = Vector3.zero;
        foreach (var a in ceilingAnchors)
            avgPos += a.transform.position;

        return avgPos / ceilingAnchors.Count;
    }

    // ============================================================
    // 🔹 WALL CENTERS
    // ============================================================
    public List<Vector3> GetLongestWallCenters(int count = 2)
    {
        if (CurrentRoom == null)
            return new List<Vector3>();

        var walls = CurrentRoom.Anchors
            .Where(a => a.Label == MRUKAnchor.SceneLabels.WALL_FACE)
            .OrderByDescending(a => EstimateAnchorSize(a))
            .Take(count)
            .ToList();

        List<Vector3> centers = new();
        foreach (var w in walls)
            centers.Add(GetAnchorPosition(w, MRUK.PositioningMethod.CENTER));

        return centers;
    }

    // ============================================================
    // 🔹 OPPOSED WALL PAIR (ideal para portales)
    // ============================================================
    public (Vector3 posA, Vector3 posB, Vector3 dirA, Vector3 dirB) GetOpposedWalls()
    {
        if (CurrentRoom == null)
            return (Vector3.zero, Vector3.zero, Vector3.forward, Vector3.back);

        var walls = CurrentRoom.Anchors
            .Where(a => a.Label == MRUKAnchor.SceneLabels.WALL_FACE)
            .ToList();

        if (walls.Count < 2)
            return (Vector3.zero, Vector3.zero, Vector3.forward, Vector3.back);

        MRUKAnchor wallA = null, wallB = null;
        float minDot = 1f;

        foreach (var a in walls)
        {
            foreach (var b in walls)
            {
                if (a == b) continue;
                float dot = Mathf.Abs(Vector3.Dot(a.transform.forward, b.transform.forward));

                if (dot < minDot)
                {
                    minDot = dot;
                    wallA = a;
                    wallB = b;
                }
            }
        }

        if (wallA == null || wallB == null)
            return (Vector3.zero, Vector3.zero, Vector3.forward, Vector3.back);

        return (
            GetAnchorPosition(wallA, MRUK.PositioningMethod.CENTER),
            GetAnchorPosition(wallB, MRUK.PositioningMethod.CENTER),
            wallA.transform.forward,
            wallB.transform.forward
        );
    }

    // ============================================================
    // 🔹 ANCHOR POSITION HANDLER
    // ============================================================
    private Vector3 GetAnchorPosition(MRUKAnchor sceneAnchor, MRUK.PositioningMethod positioningMethod)
    {
        Vector3 poseFwd = sceneAnchor.transform.forward;
        float offset = EstimateAnchorSize(sceneAnchor);

        switch (positioningMethod)
        {
            case MRUK.PositioningMethod.CENTER:
                return sceneAnchor.transform.position;

            case MRUK.PositioningMethod.EDGE:
                return sceneAnchor.transform.position + poseFwd * offset * 0.5f;

            default:
                return sceneAnchor.transform.position;
        }
    }

    // ============================================================
    // 🔹 ESTIMATE SIZE (por si TryGetVolumeBounds no existe)
    // ============================================================
    private float EstimateAnchorSize(MRUKAnchor anchor)
    {
        // Intentar encontrar un collider asociado
        Collider col = anchor.GetComponentInChildren<Collider>();
        if (col != null)
            return col.bounds.size.magnitude;

        // Si no hay collider, usar escala local
        return anchor.transform.localScale.magnitude;
    }

    // ============================================================
    // 🔹 DEBUG VISUALS
    // ============================================================
    private void OnDrawGizmos()
    {
        if (!showDebug || CurrentRoom == null)
            return;

        // Centro de habitación
        Gizmos.color = centerColor;
        Gizmos.DrawWireSphere(GetRoomCenter(), gizmoSize * 1.2f);

        // Techo
        Gizmos.color = ceilingColor;
        Gizmos.DrawWireSphere(GetCeilingCenter(), gizmoSize);

        // Paredes principales
        Gizmos.color = wallColor;
        foreach (var wall in GetLongestWallCenters())
            Gizmos.DrawWireSphere(wall, gizmoSize);
    }
}
