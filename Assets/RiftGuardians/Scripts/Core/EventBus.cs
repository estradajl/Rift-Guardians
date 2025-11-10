// Core/EventBus.cs
using System;
using UnityEngine;

public static class EventBus
{
    // ==========================================================
    //  NARRATIVE / SEQUENCE EVENTS
    // ==========================================================

    /// <summary>Se dispara cuando comienza una nueva secuencia narrativa.</summary>
    public static event Action OnSequenceStart;

    /// <summary>Se dispara cuando el techo aparece en escena.</summary>
    public static event Action OnCeilingSpawned;

    /// <summary>Se dispara cuando se instancian objetos centrales.</summary>
    public static event Action OnCenterObjectsSpawned;

    /// <summary>Se dispara cuando un diálogo llega a su fin.</summary>
    public static event Action OnDialogueEnded;

    // ==========================================================
    //  PORTAL EVENTS
    // ==========================================================

    /// <summary>Pide al PortalManager que genere los portales.</summary>
    public static event Action RequestPortalSpawn;

    /// <summary>Controla la intensidad de la onda sobre la superficie del portal (0..1).</summary>
    public static event Action<float> SetPortalSurfaceWave;

    /// <summary>Activa o desactiva el shader FX de portal.</summary>
    public static event Action<bool> TogglePortalSurfaceFX;

    // ==========================================================
    //  DEBUG EVENTS
    // ==========================================================

    /// <summary>Envia un mensaje al logger del juego.</summary>
    public static event Action<string> DebugLogMessage;


    // ==========================================================
    //  SAFE INVOKERS
    // ==========================================================

    public static void RaiseSequenceStart()
    {
        DebugLogInternal("[EventBus] → SequenceStart");
        OnSequenceStart?.Invoke();
    }

    public static void RaiseCeilingSpawned()
    {
        DebugLogInternal("[EventBus] → CeilingSpawned");
        OnCeilingSpawned?.Invoke();
    }

    public static void RaiseCenterObjectsSpawned()
    {
        DebugLogInternal("[EventBus] → CenterObjectsSpawned");
        OnCenterObjectsSpawned?.Invoke();
    }

    public static void RaiseDialogueEnded()
    {
        DebugLogInternal("[EventBus] → DialogueEnded");
        OnDialogueEnded?.Invoke();
    }

    public static void RaisePortalSpawn()
    {
        DebugLogInternal("[EventBus] → RequestPortalSpawn");
        RequestPortalSpawn?.Invoke();
    }

    public static void RaiseWave(float v)
    {
        DebugLogInternal($"[EventBus] → SetPortalSurfaceWave {v:0.00}");
        SetPortalSurfaceWave?.Invoke(v);
    }

    public static void RaiseToggleFX(bool on)
    {
        DebugLogInternal($"[EventBus] → TogglePortalSurfaceFX {(on ? "ON" : "OFF")}");
        TogglePortalSurfaceFX?.Invoke(on);
    }

    public static void Log(string msg)
    {
        DebugLogMessage?.Invoke(msg);
        DebugLogInternal(msg);
    }


    // ==========================================================
    // INTERNAL DEBUG HANDLER
    // (No reemplaza tu logger, solo lo apoya en caso de que falte)
    // ==========================================================

    private static void DebugLogInternal(string msg)
    {
#if UNITY_EDITOR
        // Solo muestra logs internos en Editor, no en build
        Debug.Log(msg);
#endif
    }
}

