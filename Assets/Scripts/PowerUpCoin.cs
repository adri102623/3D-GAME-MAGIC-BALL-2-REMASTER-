using UnityEngine;

public class PowerUpCoin : MonoBehaviour
{
    public enum PowerUpType { Maximize, Minimize }
    public PowerUpType powerUpType; // Tipo de power-up (Maximizar o Minimizar)
    public float speed = 5f; // Velocidad fija de movimiento
    private float targetZ; // Eje Z de la nave
    private bool hasTarget = false;

    public void SetTargetZ(float zPosition)
    {
        targetZ = zPosition;
        hasTarget = true;
    }

    void Update()
    {
        if (!hasTarget) return;

        // Mover la moneda hacia el eje Z de la nave
        Vector3 newPosition = transform.position;
        newPosition.z -= speed * Time.deltaTime;
        transform.position = newPosition;

        // Si pasa el eje Z de la nave, destruir la moneda
        if (transform.position.z < targetZ)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Si colisiona con la nave, aplicar el power-up y destruir la moneda
        if (other.CompareTag("Player"))
        {
            Ball ballController = FindFirstObjectByType<Ball>();
            if (ballController != null)
            {
                ballController.ApplyPowerUp();
                Debug.Log("Power-up applied: Maximize");
            }
            else
            {
                Debug.LogWarning("BallController not found!");
            }
            Destroy(gameObject);
        }
    }


}