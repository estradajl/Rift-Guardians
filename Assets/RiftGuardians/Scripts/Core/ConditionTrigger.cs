// Core/ConditionTrigger.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConditionTrigger : MonoBehaviour
{
    [SerializeField] private GameConditionManager manager;   // Puedes arrastrarlo en el Inspector
    [SerializeField] private string conditionID;
    [SerializeField] private bool triggerOnEnter = true;
    [SerializeField] private bool oneTimeOnly = true;

    private bool triggered;

    private void Awake()
    {
        // Fallback solo si el usuario no asignó un manager en el Inspector
        if (manager == null)
        {
            manager = FindFirstObjectByType<GameConditionManager>();
            if (manager == null)
            {
                Debug.LogWarning(
                    $"[{nameof(ConditionTrigger)}] No GameConditionManager found in the scene.",
                    this
                );
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter || triggered) return;

        if (other.CompareTag("Player") || other.CompareTag("Hand"))
        {
            manager?.TriggerCondition(conditionID);

            if (oneTimeOnly)
                triggered = true;
        }
    }
}
