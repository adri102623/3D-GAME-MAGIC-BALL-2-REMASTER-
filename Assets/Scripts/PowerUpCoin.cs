using UnityEngine;

public class PowerUpCoin : MonoBehaviour
{
    public enum PowerUpType { Maximize, ScaleBarrier, UnScaleBarrier}
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
        // Si colisiona con el frontTrigger, aplicar el power-up y destruir la moneda
        if (other.CompareTag("FrontTrigger"))
        {
            if (powerUpType == PowerUpType.Maximize)
            {
                Ball ballController = FindFirstObjectByType<Ball>();
                if (ballController != null)
                {
                    ballController.ApplyPowerUp();
                    Debug.Log("Power-up applied: MaximizeBall");
                }
                else
                {
                    Debug.LogWarning("Ball not found!");
                }
                Destroy(gameObject);
            }
            else if (powerUpType == PowerUpType.ScaleBarrier)
            {
                Player player = FindFirstObjectByType<Player>();
                if (player != null)
                {
                    player.ApplyPowerUp_MaxBarrier();
                    Debug.Log("Power-up applied: ScaleBarrier");
                }
                else
                {
                    Debug.LogWarning("Player not found!");
                }
                Destroy(gameObject);
            }
            else if (powerUpType == PowerUpType.UnScaleBarrier)
            {
                Player player = FindFirstObjectByType<Player>();
                if (player != null)
                {
                    player.ApplyPowerUp_MinBarrier();
                    Debug.Log("Power-up applied: UnScaleBarrier");
                }
                else
                {
                    Debug.LogWarning("Player not found!");
                }
                Destroy(gameObject);
            }
        }
    }
}