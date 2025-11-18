using UnityEngine;
using DG.Tweening;
using Meta.XR.MRUtilityKit;
using System.Linq;

[DefaultExecutionOrder(50)]
public class UniversalSurfaceSpawnerLite : MonoBehaviour
{
    public enum PlacementMode
    {
        Floating,
        AnySurface,
        VerticalSurfaces,  // for walls
        OnTopOfSurface,
        HangingDown
    }

    [Header("Surface Settings")]
    [Tooltip("MRUK label to target (CEILING, FLOOR, WALL_FACE, etc.)")]
    public MRUKAnchor.SceneLabels targetLabel = MRUKAnchor.SceneLabels.WALL_FACE;

    [Tooltip("How the prefab aligns with the surface normal.")]
    public PlacementMode placementMode = PlacementMode.VerticalSurfaces;

    [Tooltip("Distance to offset prefab from surface.")]
    public float clearanceDistance = 0.15f;

    [Tooltip("Spawn automatically once MRUK loads.")]
    public bool spawnAtStart = false;

    [Tooltip("Allow fallback to room center if no anchor found.")]
    public bool allowRoomCenterFallback = true;

    [Header("Prefab & FX")]
    public GameObject prefab;
    public ParticleSystem spawnParticles;
    public Color fxColor = Color.white;
    public float fxIntensity = 5f;

    [Header("Audio Settings (Optional)")]
    [Tooltip("Sound effect to play when object spawns.")]
    public AudioClip spawnSFX;

    [Range(0f, 1f)]
    [Tooltip("Volume of the spawn sound effect.")]
    public float sfxVolume = 1f;

    [Tooltip("Whether the sound effect should loop continuously.")]
    public bool loopSFX = false;

    [Tooltip("If true, play SFX in 3D space at spawn position. If false, play it from the prefab's AudioSource.")]
    public bool playSFXGlobally = true;

    [Header("Tween Settings")]
    public float appearDuration = 1f;
    public float appearScale = 1.2f;
    public Ease appearEase = Ease.OutBack;

    private Transform anchorCenter;
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 roomCenter;
    private bool initialized = false;

    private void Start()
    {
        StartCoroutine(WaitForMRUKReady());
    }

    private System.Collections.IEnumerator WaitForMRUKReady()
    {
        while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
            yield return new WaitForSeconds(0.2f);

        yield return new WaitForSeconds(0.25f);
        SetupAnchor();

        if (spawnAtStart)
            Spawn();
    }

    private void SetupAnchor()
    {
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            if (allowRoomCenterFallback)
                CreateAnchorCenter(transform.position + Vector3.up * 2f, Vector3.up);
            initialized = true;
            Debug.LogWarning("[SpawnerLite] No MRUK room found. Using transform fallback.");
            return;
        }

        roomCenter = room.GetRoomBounds().center;

        var anchors = room.Anchors.Where(a => a.HasLabel(targetLabel.ToString())).ToList();
        if (anchors.Count == 0)
        {
            if (allowRoomCenterFallback)
                CreateAnchorCenter(roomCenter + Vector3.up * 1.5f, Vector3.up);
            initialized = true;
            Debug.LogWarning($"[SpawnerLite] No anchors with label {targetLabel}. Using fallback.");
            return;
        }

        // ✅ Choose random wall if multiple exist
        MRUKAnchor selected = (targetLabel == MRUKAnchor.SceneLabels.WALL_FACE && anchors.Count > 1)
            ? anchors[Random.Range(0, anchors.Count)]
            : anchors[0];

        Vector3 pos = selected.transform.position;

        // ✅ Detect wall facing based on its local Y rotation (yaw)
        if (targetLabel == MRUKAnchor.SceneLabels.WALL_FACE)
        {
            float wallYaw = selected.transform.eulerAngles.y;
            Quaternion yawOnly = Quaternion.Euler(0, wallYaw, 0);
            surfaceNormal = yawOnly * Vector3.forward;
            surfaceNormal = -surfaceNormal; // face inward
        }
        else
        {
            // For floors/ceilings
            surfaceNormal = selected.transform.TransformDirection(Vector3.up);
        }

        // ✅ Flip normal if somehow pointing away from the room center
        Vector3 toCenter = (roomCenter - pos).normalized;
        if (Vector3.Dot(surfaceNormal, toCenter) < 0)
            surfaceNormal = -surfaceNormal;

        // ✅ Apply clearance offset
        pos = ApplyClearance(pos, surfaceNormal);

        CreateAnchorCenter(pos, surfaceNormal);
        initialized = true;

        Debug.Log($"[SpawnerLite] Anchor '{selected.name}' | Label={targetLabel} | Final Normal={surfaceNormal}");
    }

    private Vector3 ApplyClearance(Vector3 basePos, Vector3 normal)
    {
        Vector3 dir = placementMode switch
        {
            PlacementMode.OnTopOfSurface => normal,
            PlacementMode.HangingDown => -normal,
            PlacementMode.Floating => Vector3.zero,
            PlacementMode.VerticalSurfaces => normal,
            PlacementMode.AnySurface => normal,
            _ => normal
        };

        return basePos + dir * clearanceDistance;
    }

    private void CreateAnchorCenter(Vector3 pos, Vector3 normal)
    {
        if (anchorCenter != null)
            Destroy(anchorCenter.gameObject);

        anchorCenter = new GameObject($"{targetLabel}_Center").transform;
        anchorCenter.position = pos;
        anchorCenter.SetParent(transform);
        surfaceNormal = normal;

        Debug.DrawRay(pos, normal * 0.4f, Color.green, 5f); // visualize inward direction
    }

    public void Spawn()
    {
        if (!initialized)
        {
            Debug.LogWarning("[SpawnerLite] Not initialized yet.");
            return;
        }
        if (!prefab)
        {
            Debug.LogWarning("[SpawnerLite] No prefab assigned.");
            return;
        }

        Vector3 pos = anchorCenter != null ? anchorCenter.position : transform.position;
        Quaternion rot = GetPlacementRotation();

        GameObject instance = Instantiate(prefab, pos, rot, anchorCenter);
        instance.transform.localScale = Vector3.zero;

        // 🔹 PARTICLE FX HANDLING
        if (spawnParticles)
        {
            ParticleSystem fx = Instantiate(spawnParticles, pos, Quaternion.identity);
            fx.transform.SetParent(anchorCenter);
            fx.Play();

            if (!fx.main.loop)
                Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        // 🔹 AUDIO FX HANDLING
        if (spawnSFX)
        {
            if (playSFXGlobally)
            {
                // ✅ Play globally in 3D space (same as before)
                GameObject sfxObject = new GameObject("SpawnSFX");
                sfxObject.transform.position = pos;
                sfxObject.transform.SetParent(anchorCenter);

                AudioSource audioSource = sfxObject.AddComponent<AudioSource>();
                audioSource.clip = spawnSFX;
                audioSource.spatialBlend = 1f; // 3D sound
                audioSource.volume = sfxVolume;
                audioSource.loop = loopSFX;
                audioSource.playOnAwake = false;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = 10f;
                audioSource.Play();

                if (!loopSFX)
                    Destroy(sfxObject, spawnSFX.length + 0.25f);
            }
            else
            {
                // ✅ Play locally on prefab’s AudioSource
                AudioSource prefabAudio = instance.GetComponent<AudioSource>();
                if (prefabAudio)
                {
                    prefabAudio.clip = spawnSFX;
                    prefabAudio.volume = sfxVolume;
                    prefabAudio.loop = loopSFX;
                    prefabAudio.Play();
                }
                else
                {
                    Debug.LogWarning("[SpawnerLite] Prefab has no AudioSource component for local playback.");
                }
            }
        }

        // 🔹 LIGHT FX
        FXUtility.PulseLight(pos, fxColor, fxIntensity, 2f, 0.5f);

        // 🔹 APPEAR TWEEN
        instance.transform.DOScale(appearScale, appearDuration)
            .SetEase(appearEase)
            .OnComplete(() => instance.transform.DOScale(1f, 0.25f));
    }

    private Quaternion GetPlacementRotation()
    {
        return placementMode switch
        {
            PlacementMode.Floating => Quaternion.identity,
            PlacementMode.VerticalSurfaces => Quaternion.LookRotation(surfaceNormal, Vector3.up),
            PlacementMode.AnySurface => Quaternion.LookRotation(surfaceNormal, Vector3.up),
            PlacementMode.OnTopOfSurface => Quaternion.LookRotation(Vector3.forward, Vector3.up),
            PlacementMode.HangingDown => Quaternion.LookRotation(Vector3.forward, -Vector3.up),
            _ => Quaternion.identity
        };
    }
}
