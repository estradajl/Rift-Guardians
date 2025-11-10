// Core/NarrativeDirector.cs
using UnityEngine;
using DG.Tweening;

public class NarrativeDirector : MonoBehaviour
{
    [Header("References")]
    public CeilingEventController ceilingEvent;
    public ObjectSpawnManager objectSpawner;
    public DialogueSystem dialogueSystem;
    // public PortalManager portalManager;  // optional

    [Header("Timing")]
    public float delayAfterCeiling = 1.5f;
    public float delayAfterDialogue = 1.2f;
    public float delayBeforeStart = 0.75f;

    private bool sequenceRunning;

    private void OnEnable()
    {
        // Subscribe to event bus
        EventBus.OnCeilingSpawned += OnCeilingSpawned;
        EventBus.OnCenterObjectsSpawned += OnCenterObjectsSpawned;
        EventBus.OnDialogueEnded += OnDialogueFinished;
        EventBus.OnSequenceStart += RestartSequence;  // desde GameStateManager
    }

    private void OnDisable()
    {
        EventBus.OnCeilingSpawned -= OnCeilingSpawned;
        EventBus.OnCenterObjectsSpawned -= OnCenterObjectsSpawned;
        EventBus.OnDialogueEnded -= OnDialogueFinished;
        EventBus.OnSequenceStart -= RestartSequence;
    }

    private void Start()
    {
        RestartSequence();
    }

    // ======================================================
    // SEQUENCE FLOW
    // ======================================================

    private void RestartSequence()
    {
        if (sequenceRunning) return;
        sequenceRunning = true;

        EventBus.Log("🌀 Narrative Sequence START");

        // Delay antes de empezar (útil después de GameOver)
        DOVirtual.DelayedCall(delayBeforeStart, () =>
        {
            SpawnCeiling();
        });
    }

    private void SpawnCeiling()
    {
        EventBus.Log("✨ Spawning ceiling entity...");
        ceilingEvent.SpawnCrackAndEntity();
        // Esto dispara EventBus.RaiseCeilingSpawned();
    }

    private void OnCeilingSpawned()
    {
        DOVirtual.DelayedCall(delayAfterCeiling, () =>
        {
            EventBus.Log("🔶 Spawning center objects...");
            objectSpawner.SpawnCenterObjects();
            // Esto dispara EventBus.RaiseCenterObjectsSpawned();
        });
    }

    private void OnCenterObjectsSpawned()
    {
        // Un pequeño delay para ambientar
        DOVirtual.DelayedCall(1.0f, () =>
        {
            PlayDialogue();
        });
    }

    private void PlayDialogue()
    {
        EventBus.Log("💬 Playing intro dialogue...");

        dialogueSystem.PlayIntroDialogue(() =>
        {
            EventBus.RaiseDialogueEnded();
        });
    }

    private void OnDialogueFinished()
    {
        DOVirtual.DelayedCall(delayAfterDialogue, () =>
        {
            EventBus.Log("✅ Dialogue ended → Next stage ready!");
            // portalManager?.GeneratePortals();

            sequenceRunning = false; // Permitir nuevo start si GameStateManager lo pide
        });
    }
}
