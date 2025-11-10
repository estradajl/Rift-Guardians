// Core/GameStateManager.cs
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    // ==========================================================
    // SINGLETON
    // ==========================================================
    public static GameStateManager Instance { get; private set; }

    public enum GameState { None, Playing, Win, GameOver }

    [Header("UI Overlay")]
    public CanvasGroup fadePanel;
    public Image fadeImage;

    [Header("Colors")]
    public Color winColor = new(0f, 1f, 0.8f, 1f);
    public Color loseColor = new(1f, 0f, 0f, 1f);

    [Header("Timing")]
    public float fadeDuration = 2f;
    public float holdDuration = 2f;

    [Header("Audio")]
    public AudioClip winSFX;
    public AudioClip loseSFX;

    private GameState currentState = GameState.None;
    private AudioSource audioSource;
    private Tween fadeTween;

    private void Awake()
    {
        // ------------------------------------------------------
        // Singleton seguro
        // ------------------------------------------------------
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();

        if (fadePanel != null)
            fadePanel.alpha = 0f;

        currentState = GameState.None;
    }

    // ==========================================================
    // PLAYING STATE
    // ==========================================================
    public void SetPlayingState()
    {
        currentState = GameState.Playing;
        FadeTo(0f, 1f);
    }

    // ==========================================================
    // WIN
    // ==========================================================
    public void TriggerWin()
    {
        if (currentState == GameState.Win) return;

        currentState = GameState.Win;
        ApplyOverlayColor(winColor);

        var camPos = GetCameraPosition();
        FXUtility.PulseLight(camPos, winColor, 5f, 5f, 2f);
        FXUtility.PlayOneShot3D(winSFX, camPos);

        FadeTo(1f, fadeDuration);
        DOVirtual.DelayedCall(fadeDuration + holdDuration, () => FadeTo(0f, 1.5f));

        EventBus.Log("🏆 WIN triggered");
    }

    // ==========================================================
    // GAME OVER
    // ==========================================================
    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;
        ApplyOverlayColor(loseColor);

        var camPos = GetCameraPosition();
        FXUtility.PulseLight(camPos, loseColor, 6f, 4f, 1.5f);
        FXUtility.PlayOneShot3D(loseSFX, camPos);

        FadeTo(1f, fadeDuration);

        // Reinicio después del hold
        DOVirtual.DelayedCall(fadeDuration + holdDuration, RestartGame);

        EventBus.Log("💀 GAME OVER triggered");
    }

    // ==========================================================
    // RESET
    // ==========================================================
    private void RestartGame()
    {
        FadeTo(0f, 1.2f);
        currentState = GameState.Playing;

        // Si tienes narrativa conectada, se reinicia aquí
        EventBus.RaiseSequenceStart();
    }

    // ==========================================================
    // HELPERS
    // ==========================================================
    private void FadeTo(float targetAlpha, float duration)
    {
        fadeTween?.Kill();

        if (fadePanel == null)
            return;

        fadeTween = fadePanel.DOFade(targetAlpha, duration)
                             .SetEase(Ease.InOutSine);
    }

    private void ApplyOverlayColor(Color color)
    {
        if (fadeImage != null)
            fadeImage.color = color;
    }

    private Vector3 GetCameraPosition()
    {
        if (Camera.main != null)
            return Camera.main.transform.position;

        // fallback útil en VR
        return transform.position;
    }
}
