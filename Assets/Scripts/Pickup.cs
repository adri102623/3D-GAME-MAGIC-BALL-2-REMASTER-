using UnityEngine;

public class PickupHealth : MonoBehaviour
{
    public int vidas = 1;

    public Material material3vidas;
    public Material material2vidas;
    public Material material1vida;

    public GameObject explosionPrefab;
    private GameObject coinMaximizePrefab; // Prefab de la moneda de maximizar
    private GameObject coinScaleBarrierPrefab; // Prefab de la moneda para escalar la barrera
    private GameObject coinUnScaleBarrierPrefab; // Prefab de la moneda para escalar la barrera
    private Transform playerTransform; // Referencia a la nave
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateMaterial();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        // Cargar el prefab dinámicamente desde Resources al inicio
        coinMaximizePrefab = Resources.Load<GameObject>("Prefabs/CoinMaximize");
        coinScaleBarrierPrefab = Resources.Load<GameObject>("Prefabs/CoinScaleBarrier");
        coinUnScaleBarrierPrefab = Resources.Load<GameObject>("Prefabs/CoinUnScaleBarrier");
    }
    void SpawnPowerUp()
    {
        float randomValue = Random.value;

        if (randomValue < 0.25f) // 25% de probabilidad de maximizar la pelota
        {
            SpawnCoin(coinMaximizePrefab, transform.position);
        }
        else if (randomValue < 0.5f) // 25% de probabilidad de escalar la barrera
        {
            // Ajustar la posición para que esté en el rango [-8, 8] en X
            Vector3 spawnPosition = transform.position;
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, -8f, 8f);
            SpawnCoin(coinScaleBarrierPrefab, spawnPosition);
        }
        else if (randomValue < 0.75f) // 25% de probabilidad de escalar la barrera
        {
            // Ajustar la posición para que esté en el rango [-8, 8] en X

            SpawnCoin(coinUnScaleBarrierPrefab, transform.position);
        }
    }

    void SpawnCoin(GameObject coinPrefab, Vector3 spawnPosition)
    {
        if (playerTransform == null || coinPrefab == null)
        {
            Debug.LogWarning("Cannot spawn coin: PlayerTransform or coinPrefab is null. Retrying to find Player...");
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

    public void TocarPelota()
    {
        vidas--;
        if (vidas <= 0)
        {

            if (explosionPrefab != null)
            {
                GameObject particles = Instantiate(explosionPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            }
            SpawnPowerUp();
            Destroy(gameObject);
        }
        else
        {
            UpdateMaterial();
        }
    }

    void UpdateMaterial()
    {
        if(vidas == 3 && material3vidas != null){
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
            