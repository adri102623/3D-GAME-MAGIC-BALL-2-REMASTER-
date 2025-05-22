using UnityEngine;

public class PickupHealth : MonoBehaviour
{
    public int vidas = 1;

    public Material material3vidas;
    public Material material2vidas;
    public Material material1vida;

    public GameObject explosionPrefab;
    public GameObject coinMaximizePrefab; // Prefab de la moneda de maximizar
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
        if (coinMaximizePrefab == null)
        {
            Debug.LogWarning("CoinMaximize prefab not found in Resources/Prefabs! Please ensure the prefab exists.");
        }
    }

    void SpawnPowerUp()
    {
        // Probabilidad de 25% de generar el power-up de aumentar
        float randomValue = Random.value; // Valor entre 0 y 1

        if (randomValue < 0.25f) // 25% de probabilidad de maximizar
        {
            SpawnCoin(coinMaximizePrefab);
        }
        // 75% de probabilidad de no generar nada
    }

    void SpawnCoin(GameObject coinPrefab)
    {
        if (playerTransform == null || coinPrefab == null)
        {
            Debug.LogWarning("Cannot spawn coin: PlayerTransform or coinPrefab is null.");
            return;
        }

        // Instanciar la moneda en la posición del bloque
        GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

        // Configurar el eje Z objetivo (el de la nave)
        PowerUpCoin coinScript = coin.GetComponent<PowerUpCoin>();
        if (coinScript != null)
        {
            coinScript.SetTargetZ(playerTransform.position.z);
            Debug.Log("Coin spawned at position: " + transform.position);
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
            