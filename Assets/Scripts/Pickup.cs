using UnityEngine;

public class PickupHealth : MonoBehaviour
{
    public int vidas = 1;

    // Materiales para diferentes niveles de vida
    public Material material3vidas;
    public Material material2vidas;
    public Material material1vida;

    [Header("Efectos y Partículas")]
    public GameObject explosionPrefab;

    [Header("Configuración de PowerUps")]
    [Range(0f, 1f)]
    private float probabilidadPowerUp = 1f;
    private int numPowerUps = 6; // Cambiado de 5 a 6 para incluir CoinMagnet

    // Array para almacenar los prefabs cargados desde Resources
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

        // Cargar prefabs desde Resources
        CargarPrefabsPowerUps();
    }

    void CargarPrefabsPowerUps()
    {
        // Nombres de los prefabs en Resources/Prefabs/Upgrades/
        string[] nombresPowerUps = {
            "CoinMaximize",
            "CoinScaleBarrier",
            "CoinUnScaleBarrier",
            "CoinSpeedUp",
            "CoinUnSpeed",
            "CoinMagnet"
        };

        powerUpPrefabs = new GameObject[nombresPowerUps.Length];

        for (int i = 0; i < nombresPowerUps.Length; i++)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Upgrades/" + nombresPowerUps[i]);
            if (prefab != null)
            {
                powerUpPrefabs[i] = prefab;
                Debug.Log($"PowerUp '{nombresPowerUps[i]}' cargado exitosamente.");
            }
            else
            {
                Debug.LogWarning($"No se pudo cargar el prefab de PowerUp '{nombresPowerUps[i]}' desde Resources/Prefabs/Upgrades/");
            }
        }
    }

    void UpdateMaterial()
    {
        if (rend == null) return;

        switch (vidas)
        {
            case 3:
                if (material3vidas != null)
                    rend.material = material3vidas;
                break;
            case 2:
                if (material2vidas != null)
                    rend.material = material2vidas;
                break;
            case 1:
                if (material1vida != null)
                    rend.material = material1vida;
                break;
        }
    }

    void SpawnPowerUp()
    {
        // Verificar primero si debemos generar un power-up según la probabilidad
        if (Random.value > probabilidadPowerUp)
        {
            Debug.Log("No se generó power-up (fuera del porcentaje de probabilidad)");
            return;
        }

        // Verificar que hay prefabs disponibles
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            Debug.LogWarning("No PowerUp prefabs available!");
            return;
        }

        // Seleccionar un power-up aleatorio entre los disponibles
        int powerUpIndex = Random.Range(0, Mathf.Min(numPowerUps, powerUpPrefabs.Length));
        GameObject selectedPowerUp = powerUpPrefabs[powerUpIndex];

        if (selectedPowerUp == null)
        {
            Debug.LogWarning($"El prefab de PowerUp en el índice {powerUpIndex} es nulo.");
            return;
        }

        // Posición para el spawn
        Vector3 spawnPosition = transform.position;

        // Ajustar la posición si es necesario según el tipo de power-up
        if (powerUpIndex == 1) // CoinScaleBarrier necesita estar en el rango [-8, 8] en X
        {
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, -8f, 8f);
        }

        // Generar el power-up
        SpawnCoin(selectedPowerUp, spawnPosition);
        Debug.Log($"PowerUp generado: {selectedPowerUp.name} en posición {spawnPosition}");
    }

    void SpawnCoin(GameObject coinPrefab, Vector3 spawnPosition)
    {
        GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

        // Configurar el objetivo si el power-up tiene el script PowerUpCoin
        PowerUpCoin powerUpCoin = coin.GetComponent<PowerUpCoin>();
        if (powerUpCoin != null && playerTransform != null)
        {
            powerUpCoin.SetTargetZ(playerTransform.position.z);
        }
    }

    void IncrementScore()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.IncrementScore();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            // Simplemente llamar a TocarPelota, ya no necesitamos llamar al método de Ball
            TocarPelota();
        }
    }

    public void TocarPelota()
    {
        // Mostrar partículas con CADA impacto, no solo al final
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
}