// Core/CollectibleItem.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectibleItem : MonoBehaviour
{
    private bool collected;

    // Referencia opcional para evitar búsquedas globales
    [SerializeField] private CollectibleCondition conditionManager;

    private void Awake()
    {
        // Fallback limpio si no asignaste nada en inspector
        if (conditionManager == null)
            conditionManager = CollectibleCondition.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        // Soporte para VR: manos, controller, cuerpo, etc.
        if (!other.CompareTag("Player") && !other.CompareTag("Hand"))
            return;

        collected = true;

        if (conditionManager != null)
        {
            conditionManager.RegisterCollectiblePickup(gameObject);
        }
        else
        {
            Debug.LogWarning(
                $"CollectibleItem en '{name}' no encontró un CollectibleCondition.", this
            );
        }
    }
}
