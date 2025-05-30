using UnityEngine;

public class PickupHealth : MonoBehaviour
{
    public int vidas = 1;

    // Materiales para diferentes niveles de vida
    public Material material3vidas;
    public Material material2vidas;
    public Material material1vida;

    [Header("Efectos y Part�culas")]
    public GameObject explosionPrefab;

    [Header("Configuraci�n de PowerUps")]
    [Range(0f, 1f)]
    public float probabilidadPowerUp = 0.2f; // 20% de probabilidad por defecto
    public int numPowerUps = 3; // N�mero de power-ups disponibles

    // Referencias a los prefabs de power-ups
    private GameObject[] powerUpPrefabs;
    private Transform playerTransform;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateMaterial();

        // Buscar la nave del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Cargar los prefabs de power-ups din�micamente
        CargarPrefabsPowerUps();
    }

    void CargarPrefabsPowerUps()
    {
        // Inicializar el array con el n�mero especificado de power-ups
        powerUpPrefabs = new GameObject[numPowerUps];

        // Cargar cada prefab seg�n su �ndice
        powerUpPrefabs[0] = Resources.Load<GameObject>("Prefabs/CoinMaximize");
        powerUpPrefabs[1] = Resources.Load<GameObject>("Prefabs/CoinScaleBarrier");
        powerUpPrefabs[2] = Resources.Load<GameObject>("Prefabs/CoinUnScaleBarrier");

        // Verificar que todos los prefabs se cargaron correctamente
        for (int i = 0; i < numPowerUps; i++)
        {
            if (powerUpPrefabs[i] == null)
            {
                Debug.LogWarning($"No se pudo cargar el prefab de PowerUp en el �ndice {i}. Aseg�rate de que existe en la carpeta Resources.");
            }
        }
    }

    void SpawnPowerUp()
    {
        // Verificar primero si debemos generar un power-up seg�n la probabilidad
        if (Random.value > probabilidadPowerUp)
        {
            Debug.Log("No se gener� power-up (fuera del porcentaje de probabilidad)");
            return;
        }

        // Seleccionar un power-up aleatorio entre los disponibles
        int powerUpIndex = Random.Range(0, numPowerUps);
        GameObject selectedPowerUp = powerUpPrefabs[powerUpIndex];

        if (selectedPowerUp == null)
        {
            Debug.LogWarning($"El prefab de PowerUp en el �ndice {powerUpIndex} es nulo.");
            return;
        }

        // Posici�n para el spawn
        Vector3 spawnPosition = transform.position;

        // Ajustar la posici�n si es necesario seg�n el tipo de power-up
        if (powerUpIndex == 1) // CoinScaleBarrier necesita estar en el rango [-8, 8] en X
        {
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, -8f, 8f);
        }

        // Generar el power-up
        SpawnCoin(selectedPowerUp, spawnPosition);
        Debug.Log($"PowerUp generado: {selectedPowerUp.name} en posici�n {spawnPosition}");
    }

    void SpawnCoin(GameObject coinPrefab, Vector3 spawnPosition)
    {
        if (playerTransform == null || coinPrefab == null)
        {
            Debug.LogWarning("Cannot spawn coin: PlayerTransform or coinPrefab is null. Retrying to find Player...");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            if (playerTransform == null || coinPrefab == null)
            {
                Debug.LogError("Failed to spawn coin after retry. Check Player and prefab setup.");
                return;
            }
        }

        GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

        PowerUpCoin coinScript = coin.GetComponent<PowerUpCoin>();
        if (coinScript != null)
        {
            coinScript.SetTargetZ(playerTransform.position.z);
            Debug.Log("Coin spawned at position: " + spawnPosition);
        }
        else
        {
            Debug.LogWarning("PowerUpCoin script not found on instantiated coin!");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            TocarPelota();
        }
    }


    public void set_1()
    {
        vidas = 1;
    }
    public void TocarPelota()
    {
        // Mostrar part�culas con CADA impacto, no solo al final
        if (explosionPrefab != null)
        {
            GameObject particles = Instantiate(explosionPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        }

        // Incrementar puntuación
        IncrementScore();
        vidas--;
        if (vidas <= 0)
        {
            // Reproducir sonido de destrucción
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDestroySound();
            }
            SpawnPowerUp();
            Destroy(gameObject);
        }
        else
        {
            // Reproducir sonido de destrucción
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHitSound();
            }
            UpdateMaterial();
        }
    }

    private void IncrementScore()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.IncrementScore();
        }
        else
        {
            Debug.LogWarning("ScoreManager not found! Make sure ScoreManager is in the scene.");
        }
    }

    void UpdateMaterial()
    {
        if (vidas == 3 && material3vidas != null)
        {
            rend.material = material3vidas;
        }
        else if (vidas == 2 && material2vidas != null)
        {
            rend.material = material2vidas;
        }
        else if (vidas == 1 && material1vida != null)
        {
            rend.material = material1vida;
        }
    }
}