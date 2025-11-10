// Core/TimeCondition.cs
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class TimeCondition : MonoBehaviour
{
    [Header("Timer Settings")]
    public float duration = 60f;               // duración total en segundos
    public bool startOnEnable = true;          // comienza automáticamente
    public bool triggerLoseOnTimeout = true;   // si el tiempo se agota, perder
    public bool triggerWinOnTimeout = false;   // o ganar

    [Header("UI (opcional)")]
    public TextMeshProUGUI timerText;                     // referencia a texto UI para mostrar tiempo
    public Image timerBar;                     // si usas barra de progreso (fillAmount)

    [Header("Visual FX")]
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    public float warningThreshold = 0.3f;      // 30% del tiempo
    public float dangerThreshold = 0.1f;       // 10% del tiempo

    private float remaining;
    private bool active;
    private GameConditionManager conditionManager;

    void Start()
    {
        conditionManager = FindObjectOfType<GameConditionManager>();
        if (startOnEnable)
            StartTimer();
    }

    public void StartTimer()
    {
        remaining = duration;
        active = true;
        UpdateUI();
        InvokeRepeating(nameof(Tick), 1f, 1f);
    }

    public void StopTimer()
    {
        active = false;
        CancelInvoke(nameof(Tick));
    }

    private void Tick()
    {
        if (!active) return;

        remaining -= 1f;
        UpdateUI();

        if (remaining <= 0f)
        {
            StopTimer();
            if (triggerLoseOnTimeout)
                conditionManager.TriggerCondition("TimeExpired");
            else if (triggerWinOnTimeout)
                conditionManager.TriggerCondition("TimeCompleted");
        }
    }

    private void UpdateUI()
    {
        if (timerText)
            timerText.text = Mathf.CeilToInt(remaining).ToString();

        if (timerBar)
        {
            float t = remaining / duration;
            timerBar.fillAmount = t;

            if (t < dangerThreshold)
                timerBar.color = dangerColor;
            else if (t < warningThreshold)
                timerBar.color = warningColor;
        }
    }
}



/*🎬 Ejemplo de integración narrativa

Inicia el temporizador al terminar el diálogo:

EventBus.OnDialogueEnded += () => FindObjectOfType<TimeCondition>().StartTimer();


Si el jugador no entra al portal correcto antes de que acabe el tiempo → Game Over.*/