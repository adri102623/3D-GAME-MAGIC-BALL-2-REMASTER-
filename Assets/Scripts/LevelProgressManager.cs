using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }
    
    [Header("Level Progress")]
    private int totalPickups = 0;
    private int destroyedPickups = 0;
    
    [Header("CoinNextLevel Settings")]
    public int pickupsToSpawnTrophy = 1;
    private bool trophySpawned = false;
    
    [Header("Debug")]
    public bool showProgressDebug = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // NO usar DontDestroyOnLoad para que se resetee en cada nivel
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
            Debug.Log($"LevelProgressManager: Level started with {totalPickups} total pickups. Trophy will spawn after destroying {pickupsToSpawnTrophy} pickup(s).");
        }
    }
    
    void CountTotalPickups()
    {
        // CORREGIDO: Usar FindObjectsByType en lugar de FindObjectsOfType
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("PickUp");
        totalPickups = pickups.Length;
        destroyedPickups = 0;
        trophySpawned = false; // Resetear estado del trofeo
        
        // También buscar objetos con el componente PickupHealth directamente
        PickupHealth[] pickupHealthComponents = FindObjectsByType<PickupHealth>(FindObjectsSortMode.None);
        if (pickupHealthComponents.Length > pickups.Length)
        {
            totalPickups = pickupHealthComponents.Length;
        }
    }
    
    public void OnPickupDestroyed()
    {
        destroyedPickups++;
        
        float percentage = GetCompletionPercentage();
        
        if (showProgressDebug)
        {
            Debug.Log($"PROGRESS: {destroyedPickups}/{totalPickups} destroyed ({percentage:F1}%)");
        }
        
        // NUEVO: Verificar si debe aparecer el trofeo usando contador
        if (!trophySpawned && destroyedPickups >= pickupsToSpawnTrophy)
        {
            SpawnTrophy();
        }
        
        // Verificar si se completó el nivel
        if (destroyedPickups >= totalPickups)
        {
            OnLevelCompleted();
        }
    }
    
    // Método para spawnear el trofeo
    void SpawnTrophy()
    {
        trophySpawned = true;
        
        Debug.Log($"TROPHY SPAWNING: {destroyedPickups} pickups destroyed, spawning CoinNextLevel trophy!");
        
        // Cargar el prefab desde Resources
        GameObject trophyPrefab = Resources.Load<GameObject>("Prefabs/Upgrades/CoinNextLevel");
        if (trophyPrefab == null)
        {
            Debug.LogError("LevelProgressManager: CoinNextLevel prefab not found in Resources/Prefabs/Upgrades/");
            return;
        }
        
        // Encontrar una posición adecuada para spawnear
        Vector3 spawnPosition = FindTrophySpawnPosition();
        
        // Instanciar el trofeo
        GameObject trophy = Instantiate(trophyPrefab, spawnPosition, Quaternion.identity);
        
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
        
        Debug.Log($"TROPHY SPAWNED: CoinNextLevel trophy spawned at position {spawnPosition}");
    }
    
    // NUEVO: Método para encontrar posición del trofeo
    Vector3 FindTrophySpawnPosition()
    {
        // Buscar el player para determinar una buena posición
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            // Spawnear delante del jugador, elevado como CoinScaleBarrier
            return new Vector3(
                Random.Range(-3f, 3f), // X aleatoria cerca del centro
                playerPos.y + 3f,      // Y elevada (como CoinScaleBarrier)
                playerPos.z + 15f      // Z adelante del jugador
            );
        }
        
        // Posición por defecto si no se encuentra el jugador
        return new Vector3(0f, 3f, 10f);
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
        
        if (SceneTransitionManager.Instance != null)
        {
            // Pequeño delay antes de cargar el siguiente nivel
            Invoke(nameof(LoadNextLevel), 2f);
        }
    }
    
    void LoadNextLevel()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadNextLevel();
        }
    }
    
    // Método para obtener información de progreso (útil para UI)
    public string GetProgressString()
    {
        return $"{destroyedPickups}/{totalPickups} ({GetCompletionPercentage():F1}%)";
    }
    
    // Método para debug manual
    [ContextMenu("Show Current Progress")]
    public void ShowCurrentProgress()
    {
        Debug.Log($"Current Progress: {GetProgressString()}");
        Debug.Log($"Trophy spawned: {trophySpawned}");
        Debug.Log($"Pickups needed for trophy: {pickupsToSpawnTrophy}");
    }
    
    // Método para recontear pickups (útil si se crean dinámicamente)
    public void RecountPickups()
    {
        CountTotalPickups();
        Debug.Log($"LevelProgressManager: Recounted pickups. New total: {totalPickups}");
    }
    
    // NUEVO: Método para cambiar el contador dinámicamente
    public void SetPickupsNeededForTrophy(int count)
    {
        pickupsToSpawnTrophy = count;
        Debug.Log($"Trophy spawn requirement changed to: {pickupsToSpawnTrophy} pickups");
    }
    
    // NUEVO: Verificar si el trofeo ya fue spawneado
    public bool IsTrophySpawned()
    {
        return trophySpawned;
    }
}