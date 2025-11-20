using UnityEngine;

public class CityFXBinder : MonoBehaviour
{
    public CityWorldController city;
    public WorldSectionSimpleFXController clippingFX;

    [Header("FX Modes")]
    public WorldSectionSimpleFXController.FillerFXMode moveFX = WorldSectionSimpleFXController.FillerFXMode.Scan;
    public WorldSectionSimpleFXController.FillerFXMode zoomFX = WorldSectionSimpleFXController.FillerFXMode.Pulse;
    public WorldSectionSimpleFXController.FillerFXMode idleFX = WorldSectionSimpleFXController.FillerFXMode.Shell;
    private WorldSectionSimpleFXController.FillerFXMode currentMode;

    [Header("Block/Bounce FX")]
    public WorldSectionSimpleFXController.FillerFXMode borderFX = WorldSectionSimpleFXController.FillerFXMode.Pulse;
    public float borderFXDuration = 0.35f;

    [Header("Zoom Sensitivity")]
    public float zoomThreshold = 0.001f;

    private float borderFXTimer = 0f;

    void Update()
    {
        if (!city || !clippingFX) return;

        // ==========================================
        // 1. BORDER MODE
        // ==========================================
        if (city.IsAtBorder)
        {
            borderFXTimer = borderFXDuration;
            SetFX(borderFX);
            return;
        }

        if (borderFXTimer > 0)
        {
            borderFXTimer -= Time.deltaTime;
            SetFX(borderFX);
            return;
        }

        // ==========================================
        // 2. ZOOM FX
        // ==========================================
        if (Mathf.Abs(city.ZoomVelocity) > zoomThreshold)
        {
            SetFX(zoomFX);
            return;
        }

        // ==========================================
        // 3. MOVE FX
        // ==========================================
        if (city.MoveInput.sqrMagnitude > 0.001f)
        {
            SetFX(moveFX);
            return;
        }

        // ==========================================
        // 4. IDLE FX
        // ==========================================
        SetFX(idleFX);
    }

    // ---------------------------
    // CENTRALIZADOR DE CAMBIO FX
    // ---------------------------
    private void SetFX(WorldSectionSimpleFXController.FillerFXMode mode)
    {
        if (currentMode == mode)
            return;

        currentMode = mode;
        clippingFX.SetFX(mode);

        Debug.Log("FX MODE → " + currentMode);
    }

    public WorldSectionSimpleFXController.FillerFXMode GetCurrentMode()
    {
        return currentMode;
    }
}
