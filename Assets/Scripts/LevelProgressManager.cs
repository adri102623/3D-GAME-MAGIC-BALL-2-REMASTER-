using UnityEngine;
using System.Collections.Generic; // Asegurando el uso de listas

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }
    
    [Header("Level Progress")]
    private int totalPickups = 0;
    private int destroyedPickups = 0;
    
    [Header("CoinNextLevel Settings")]
    [Range(0f, 100f)]
    public float trophySpawnPercentage = 90f;
    private bool trophySpawned = false;
    
    [Header("Debug")]
    public bool showProgressDebug = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Contar todos los pickups al inicio del nivel
        CountTotalPickups();
        
        if (showProgressDebug)
        {
            Debug.Log($"LevelProgressManager: Level started with {totalPickups} total pickups. Trophy will spawn at {trophySpawnPercentage}% completion.");
        }
    }
    
    void CountTotalPickups()
    {
        // Buscar todos los objetos con tag "PickUp"
        GameObject[] allPickups = GameObject.FindGameObjectsWithTag("PickUp");
        
        // Filtrar para obtener solo los objetos principales (no hijos)
        List<GameObject> mainPickups = new List<GameObject>();
        
        foreach (GameObject pickup in allPickups)
        {
            // Solo contar si es un objeto principal (no tiene padre con tag PickUp)
            bool isMainPickup = true;
            Transform parent = pickup.transform.parent;
            
            // Verificar si algún padre tiene el tag PickUp
            while (parent != null)
            {
                if (parent.CompareTag("PickUp"))
                {
                    isMainPickup = false;
                    break;
                }
                parent = parent.parent;
            }
            
            // Solo añadir si es un pickup principal Y tiene el componente PickupHealth
            if (isMainPickup && pickup.GetComponent<PickupHealth>() != null)
            {
                mainPickups.Add(pickup);
            }
        }
        
        totalPickups = mainPickups.Count;
        destroyedPickups = 0;
        trophySpawned = false;
        
        // Debug detallado para identificar el conteo correcto
        Debug.Log($"=== PICKUP COUNT DEBUG ===");
        Debug.Log($"Total objects with tag 'PickUp': {allPickups.Length}");
        Debug.Log($"Main pickups (not children): {mainPickups.Count}");
        Debug.Log($"Final totalPickups count: {totalPickups}");
        
        // Listar todos los pickups principales para debug
        for (int i = 0; i < mainPickups.Count; i++)
        {
            if (mainPickups[i] != null)
            {
                PickupHealth ph = mainPickups[i].GetComponent<PickupHealth>();
                Debug.Log($"Main Pickup {i}: {mainPickups[i].name}, Position: {mainPickups[i].transform.position}, Lives: {(ph != null ? ph.vidas : 0)}");
            }
        }
        
        if (showProgressDebug)
        {
            Debug.Log($"LevelProgressManager: Found {totalPickups} main pickups in scene");
        }
    }
    
    public void OnPickupDestroyed()
    {
        destroyedPickups++;
        
        float percentage = GetCompletionPercentage();
        
        if (showProgressDebug)
        {
            Debug.Log($"PROGRESS UPDATE: {destroyedPickups}/{totalPickups} destroyed ({percentage:F1}%) - Trophy spawned: {trophySpawned}");
        }
        
        // MEJORADO: Verificar si debe aparecer el trofeo con mejor lógica
        if (!trophySpawned && percentage >= trophySpawnPercentage)
        {
            Debug.Log($"TROPHY TRIGGER: Percentage {percentage:F1}% >= {trophySpawnPercentage}% - Spawning trophy!");
            SpawnTrophy();
        }
        
        // Verificar si se completó el nivel (100%)
        if (destroyedPickups >= totalPickups)
        {
            OnLevelCompleted();
        }
    }
    
    // MEJORADO: Método para spawnear el trofeo con mejor validación
    void SpawnTrophy()
    {
        if (trophySpawned)
        {
            Debug.LogWarning("Trophy already spawned! Skipping...");
            return;
        }
        
        trophySpawned = true;
        
        Debug.Log($"TROPHY SPAWNING: {destroyedPickups} pickups destroyed ({GetCompletionPercentage():F1}%), spawning CoinNextLevel trophy!");
        
        // Cargar el prefab desde Resources
        GameObject trophyPrefab = Resources.Load<GameObject>("Prefabs/Upgrades/CoinNextLevel");
        if (trophyPrefab == null)
        {
            Debug.LogError("LevelProgressManager: CoinNextLevel prefab not found in Resources/Prefabs/Upgrades/");
            trophySpawned = false; // Resetear si falla
            return;
        }
        
        // Encontrar una posición adecuada para spawnear
        Vector3 spawnPosition = FindTrophySpawnPosition();
        
        // Instanciar el trofeo
        GameObject trophy = Instantiate(trophyPrefab, spawnPosition, Quaternion.identity);
        
        if (trophy == null)
        {
            Debug.LogError("Failed to instantiate trophy!");
            trophySpawned = false;
            return;
        }
        
        // Configurar el PowerUpCoin component
        PowerUpCoin powerUpCoin = trophy.GetComponent<PowerUpCoin>();
        if (powerUpCoin != null)
        {
            // Asegurar que es tipo NextLevel
            powerUpCoin.powerUpType = PowerUpCoin.PowerUpType.NextLevel;
            
            // Configurar target si hay player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                powerUpCoin.SetTargetZ(player.transform.position.z);
                Debug.Log($"Trophy configured with player target Z: {player.transform.position.z}");
            }
        }
        else
        {
            Debug.LogError("Trophy doesn't have PowerUpCoin component!");
        }
        
        Debug.Log($"TROPHY SPAWNED SUCCESSFULLY: CoinNextLevel trophy spawned at position {spawnPosition}");
    }
    
    // MEJORADO: Método para encontrar posición del trofeo
    Vector3 FindTrophySpawnPosition()
    {
        // Buscar el player para determinar una buena posición
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            // Spawnear delante del jugador, elevado
            Vector3 position = new Vector3(
                Random.Range(-2f, 2f), // X aleatoria cerca del centro  
                playerPos.y + 3f,      // Y elevada
                playerPos.z + 12f      // Z adelante del jugador
            );
            
            Debug.Log($"Trophy spawn position calculated: {position} (relative to player at {playerPos})");
            return position;
        }
        
        // Posición por defecto si no se encuentra el jugador
        Vector3 defaultPos = new Vector3(0f, 3f, 10f);
        Debug.Log($"Using default trophy position: {defaultPos}");
        return defaultPos;
    }
    
    public float GetCompletionPercentage()
    {
        if (totalPickups == 0) return 100f;
        return (float)destroyedPickups / totalPickups * 100f;
    }
    
    public int GetDestroyedCount()
    {
        return destroyedPickups;
    }
    
    public int GetTotalCount()
    {
        return totalPickups;
    }
    
    void OnLevelCompleted()
    {
        Debug.Log($"LEVEL COMPLETED! All {totalPickups} pickups destroyed (100%)");
        
        // Si no se spawneó el trofeo por alguna razón, spawnearlo ahora
        if (!trophySpawned)
        {
            Debug.Log("Trophy wasn't spawned yet - spawning it now!");
            SpawnTrophy();
        }
        
        if (SceneTransitionManager.Instance != null)
        {
            // Pequeño delay antes de cargar el siguiente nivel
            Invoke(nameof(LoadNextLevel), 3f); // Más tiempo para recoger el trofeo
        }
    }
    
    void LoadNextLevel()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadNextLevel();
        }
    }
    
    // AÑADIDO: Método para forzar spawn del trofeo (para testing)
    [ContextMenu("Force Spawn Trophy")]
    public void ForceSpawnTrophy()
    {
        if (!trophySpawned)
        {
            Debug.Log("MANUAL TROPHY SPAWN - Forcing trophy spawn");
            SpawnTrophy();
        }
        else
        {
            Debug.Log("Trophy already spawned!");
        }
    }
    
    // Método para obtener información de progreso (útil para UI)
    public string GetProgressString()
    {
        return $"{destroyedPickups}/{totalPickups} ({GetCompletionPercentage():F1}%)";
    }
    
    // MEJORADO: Método para debug manual
    [ContextMenu("Show Current Progress")]
    public void ShowCurrentProgress()
    {
        Debug.Log($"=== LEVEL PROGRESS DEBUG ===");
        Debug.Log($"Current Progress: {GetProgressString()}");
        Debug.Log($"Trophy spawned: {trophySpawned}");
        Debug.Log($"Trophy spawn percentage: {trophySpawnPercentage}%");
        Debug.Log($"Should spawn trophy: {GetCompletionPercentage() >= trophySpawnPercentage && !trophySpawned}");
    }
    
    // Método para recontear pickups (útil si se crean dinámicamente)
    public void RecountPickups()
    {
        CountTotalPickups();
        Debug.Log($"LevelProgressManager: Recounted pickups. New total: {totalPickups}");
    }
    
    // MODIFICADO: Método para cambiar el porcentaje dinámicamente
    public void SetTrophySpawnPercentage(float percentage)
    {
        trophySpawnPercentage = Mathf.Clamp(percentage, 0f, 100f);
        Debug.Log($"Trophy spawn requirement changed to: {trophySpawnPercentage}%");
    }
    
    // Verificar si el trofeo ya fue spawneado
    public bool IsTrophySpawned()
    {
        return trophySpawned;
    }

    [ContextMenu("Check Remaining Pickups")]
    public void CheckRemainingPickups()
    {
        // Usar la misma lógica de filtrado que en CountTotalPickups
        GameObject[] allPickups = GameObject.FindGameObjectsWithTag("PickUp");
        List<GameObject> mainPickups = new List<GameObject>();
        
        foreach (GameObject pickup in allPickups)
        {
            // Solo contar si es un objeto principal (no tiene padre con tag PickUp)
            bool isMainPickup = true;
            Transform parent = pickup.transform.parent;
            
            while (parent != null)
            {
                if (parent.CompareTag("PickUp"))
                {
                    isMainPickup = false;
                    break;
                }
                parent = parent.parent;
            }
            
            if (isMainPickup && pickup.GetComponent<PickupHealth>() != null)
            {
                mainPickups.Add(pickup);
            }
        }
        
        Debug.Log($"=== REMAINING PICKUPS CHECK ===");
        Debug.Log($"Total pickups counted at start: {totalPickups}");
        Debug.Log($"Pickups destroyed so far: {destroyedPickups}");
        Debug.Log($"Current progress: {GetCompletionPercentage():F1}%");
        Debug.Log($"Total objects with tag 'PickUp' in scene: {allPickups.Length}");
        Debug.Log($"Main pickups still in scene: {mainPickups.Count}");
        
        if (mainPickups.Count > 0)
        {
            Debug.Log("Remaining main pickups details:");
            for (int i = 0; i < mainPickups.Count; i++)
            {
                if (mainPickups[i] != null)
                {
                    PickupHealth ph = mainPickups[i].GetComponent<PickupHealth>();
                    Debug.Log($"Remaining pickup {i}: {mainPickups[i].name}, Position: {mainPickups[i].transform.position}, Lives: {(ph != null ? ph.vidas : 0)}");
                }
            }
        }
        else
        {
            Debug.Log("No main pickups remaining in scene - level should be complete!");
            if (destroyedPickups < totalPickups)
            {
                Debug.LogError($"ERROR: No main pickups in scene but counter shows {destroyedPickups}/{totalPickups}. Adjusting count...");
                destroyedPickups = totalPickups;
                OnLevelCompleted();
            }
        }
    }
}