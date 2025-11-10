// Core/GameConditionManager.cs
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameConditionManager : MonoBehaviour
{
    public static GameConditionManager Instance { get; private set; }

    public enum ConditionType { Win, Lose }

    [System.Serializable]
    public class GameCondition
    {
        public string conditionID;
        public ConditionType type;

        [Tooltip("Si es true, puede activarse varias veces.")]
        public bool autoReset = false;

        [Tooltip("Acciones adicionales al dispararse la condición.")]
        public UnityEvent onTriggered;

        [HideInInspector] public bool triggered;
    }

    [Header("Conditions")]
    public List<GameCondition> conditions = new();

    private Dictionary<string, GameCondition> lookup;
    private GameStateManager gameState;

    private void Awake()
    {
        // Singleton opcional (seguro y simple)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gameState = GameStateManager.Instance;
        if (!gameState)
            gameState = FindObjectOfType<GameStateManager>();

        BuildLookupTable();

        // Ejemplos de eventos narrativos
        EventBus.OnDialogueEnded += () => TriggerCondition("DialogueCompleted");
        EventBus.RequestPortalSpawn += () => TriggerCondition("PortalesAparecen");
    }

    private void BuildLookupTable()
    {
        lookup = new Dictionary<string, GameCondition>();

        foreach (var c in conditions)
        {
            if (string.IsNullOrWhiteSpace(c.conditionID))
            {
                Debug.LogWarning("GameCondition con ID vacío.", this);
                continue;
            }

            if (lookup.ContainsKey(c.conditionID))
            {
                Debug.LogWarning($"Condición duplicada: {c.conditionID}", this);
                continue;
            }

            lookup.Add(c.conditionID, c);
        }
    }

    // ==========================================================
    // MAIN METHOD
    // ==========================================================
    public void TriggerCondition(string conditionID)
    {
        if (!lookup.TryGetValue(conditionID, out var cond))
        {
            Debug.LogWarning($"⚠️ No existe la condición: {conditionID}");
            return;
        }

        if (cond.triggered && !cond.autoReset)
            return;

        cond.triggered = true;

        EventBus.Log($"🎯 Condition triggered → {conditionID}");

        cond.onTriggered?.Invoke();

        // Acción central Win/Lose
        if (gameState != null)
        {
            switch (cond.type)
            {
                case ConditionType.Win:
                    gameState.TriggerWin();
                    break;

                case ConditionType.Lose:
                    gameState.TriggerGameOver();
                    break;
            }
        }
    }

    // ==========================================================
    // RESET METHODS
    // ==========================================================
    public void ResetCondition(string conditionID)
    {
        if (lookup.TryGetValue(conditionID, out var cond))
            cond.triggered = false;
    }

    public void ResetAll()
    {
        foreach (var condition in conditions)
            condition.triggered = false;
    }
}
