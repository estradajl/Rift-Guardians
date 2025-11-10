// Core/CollectibleCondition.cs
using UnityEngine;

public class CollectibleCondition : MonoBehaviour
{
    // ==========================================================
    // SINGLETON
    // ==========================================================
    public static CollectibleCondition Instance { get; private set; }

    [Header("Collection Settings")]
    [Tooltip("Cantidad total de objetos a recolectar.")]
    public int totalCollectibles = 5;

    [Tooltip("Tag de los objetos recolectables.")]
    public string collectibleTag = "Collectible";

    [Tooltip("ID de condición que se dispara cuando se recolectan todos.")]
    public string winConditionID = "AllCollected";

    [Tooltip("Si es true, dispara condición de derrota si todos los objetos desaparecen sin recoger.")]
    public bool triggerLoseIfAllDestroyed = false;

    [Header("FX Settings")]
    public AudioClip collectSFX;
    public AudioClip allCollectedSFX;
    public Color collectPulseColor = Color.cyan;

    private int collectedCount;
    private int destroyedCount;

    private GameConditionManager conditionManager;
    private bool collectionComplete;

    private void Awake()
    {
        // -----------------------------------------------------
        // Singleton seguro (igual que GameState y ConditionMgr)
        // -----------------------------------------------------
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        conditionManager = GameConditionManager.Instance;

        if (!conditionManager)
            Debug.LogError("❌ CollectibleCondition: No existe GameConditionManager.Instance en escena.");

        // Autodetecta la cantidad si no se definió manualmente
        if (totalCollectibles <= 0)
        {
            totalCollectibles = GameObject.FindGameObjectsWithTag(collectibleTag).Length;
            EventBus.Log($"🔍 Autocontando collectibles: {totalCollectibles}");
        }

        collectedCount = 0;
        destroyedCount = 0;
        collectionComplete = false;
    }

    // ==========================================================
    //  REGISTRO: PICKUP / DESTRUCCIÓN
    // ==========================================================
    public void RegisterCollectiblePickup(GameObject obj)
    {
        if (collectionComplete) return;

        collectedCount++;
        PlayPickupFX(obj.transform.position);

        Destroy(obj);

        if (collectedCount >= totalCollectibles)
            CompleteCollection();
    }

    public void RegisterCollectibleDestroyed()
    {
        if (collectionComplete) return;

        destroyedCount++;

        if (triggerLoseIfAllDestroyed && destroyedCount >= totalCollectibles)
            TriggerLoseCondition();
    }

    // ==========================================================
    //  ACCIONES PRINCIPALES
    // ==========================================================
    private void CompleteCollection()
    {
        collectionComplete = true;

        EventBus.Log($"✨ Todos los {totalCollectibles} recolectados.");
        FXUtility.PlayOneShot3D(allCollectedSFX, GetCameraPosition());

        conditionManager.TriggerCondition(winConditionID);
    }

    private void TriggerLoseCondition()
    {
        collectionComplete = true;

        EventBus.Log("❌ Todos los collectibles se perdieron sin ser recogidos.");
        conditionManager.TriggerCondition("AllDestroyed");
    }

    // ==========================================================
    //  FX / HELPERS
    // ==========================================================
    private void PlayPickupFX(Vector3 position)
    {
        FXUtility.PulseLight(position, collectPulseColor, 3f, 2f, 0.5f);
        FXUtility.PlayOneShot3D(collectSFX, position);
    }

    private Vector3 GetCameraPosition()
    {
        if (Camera.main != null)
            return Camera.main.transform.position;

        return transform.position;
    }
}
