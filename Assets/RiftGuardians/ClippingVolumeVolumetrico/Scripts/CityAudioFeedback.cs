using UnityEngine;

public class CityAudioFeedback : MonoBehaviour
{
    [Header("Dependencies")]
    public CityFXBinder fxBinder;

    [Header("Audio Sources")]
    public AudioSource loopSource;     // Para MOVE / IDLE
    public AudioSource oneShotSource;  // Para ZOOM / BORDER

    [Header("Loop Clips")]
    public AudioClip moveLoop;
    public AudioClip idleLoop;

    [Header("One Shot Clips")]
    public AudioClip zoomSFX;
    public AudioClip borderSFX;

    [Header("Fade Settings")]
    public float fadeSpeed = 4f;

    private WorldSectionSimpleFXController.FillerFXMode lastMode;

    void Start()
    {
        lastMode = fxBinder.GetCurrentMode();
    }

    void Update()
    {
        var mode = fxBinder.GetCurrentMode();

        if (mode == lastMode)
        {
            UpdateLoopVolume(mode);
            return;
        }

        // Cambió el modo FX
        lastMode = mode;
        HandleModeChange(mode);
    }

    // --------------------------
    // MODE SWITCH
    // --------------------------
    void HandleModeChange(WorldSectionSimpleFXController.FillerFXMode mode)
    {
        switch (mode)
        {
            case WorldSectionSimpleFXController.FillerFXMode.Scan: // MOVE
                PlayLoop(moveLoop);
                break;

            case WorldSectionSimpleFXController.FillerFXMode.Pulse: // ZOOM o BORDER
                PlayOneShot(mode == fxBinder.borderFX ? borderSFX : zoomSFX);
                break;

            case WorldSectionSimpleFXController.FillerFXMode.Shell: // IDLE
                PlayLoop(idleLoop);
                break;
        }
    }

    // --------------------------
    // LOOP LOGIC
    // --------------------------
    void PlayLoop(AudioClip clip)
    {
        if (loopSource.clip == clip)
            return;

        loopSource.clip = clip;
        loopSource.Play();
    }

    // --------------------------
    // ONE SHOT LOGIC
    // --------------------------
    void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        oneShotSource.PlayOneShot(clip);
    }

    // --------------------------
    // SMOOTH FADE
    // --------------------------
    void UpdateLoopVolume(WorldSectionSimpleFXController.FillerFXMode mode)
    {
        float targetVol =
            (mode == WorldSectionSimpleFXController.FillerFXMode.Scan ||
             mode == WorldSectionSimpleFXController.FillerFXMode.Shell)
            ? 1f : 0f;

        loopSource.volume = Mathf.Lerp(loopSource.volume, targetVol, Time.deltaTime * fadeSpeed);
    }
}
