using UnityEngine;

public class PowerUpCoin : MonoBehaviour
{
    public enum PowerUpType
    {
        Maximize,
        ScaleBarrier,
        UnScaleBarrier,
        SpeedUp,
        SpeedDown,
        Magnet,
        PowerBall
    }
    
    public PowerUpType powerUpType; // Tipo de power-up
    public float speed = 5f; // Velocidad fija de movimiento
    [Header("Rotación")]
    public float rotationSpeed = 50f; // Velocidad de rotación en grados por segundo
    public Vector3 rotationAxis = Vector3.up; // Eje de rotación (Y por defecto)
    
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

        // Rotar la moneda sobre sí misma
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        // Si pasa el eje Z de la nave, destruir la moneda
        if (transform.position.z < targetZ)
        {
            Destroy(gameObject);
        }
    }

    private void IncrementScoreForPowerUp()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.IncrementScoreForPowerUp();
        }
        else
        {
            Debug.LogWarning("ScoreManager not found! Make sure ScoreManager is in the scene.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Si colisiona con el frontTrigger, aplicar el power-up y destruir la moneda
        if (other.CompareTag("FrontTrigger") || other.CompareTag("Player"))
        {
            // Reproducir sonido de power-up
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUpSound();
            }

            IncrementScoreForPowerUp();
            
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
            else if (powerUpType == PowerUpType.PowerBall)
            {
                Ball ballController = FindFirstObjectByType<Ball>();
                if (ballController != null)
                {
                    ballController.ApplyPowerUp_PowerBall();
                    Debug.Log("Power-up applied: PowerBall");
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
            else if (powerUpType == PowerUpType.SpeedUp)
            {
                Ball ballController = FindFirstObjectByType<Ball>();
                if (ballController != null)
                {
                    ballController.ApplySpeedUp();
                    Debug.Log("Power-up applied: SpeedUp (1.5x speed)");
                }
                else
                {
                    Debug.LogWarning("Ball not found!");
                }
                Destroy(gameObject);
            }
            else if (powerUpType == PowerUpType.SpeedDown)
            {
                Ball ballController = FindFirstObjectByType<Ball>();
                if (ballController != null)
                {
                    ballController.ApplySpeedDown();
                    Debug.Log("Power-up applied: SpeedDown (0.65x speed)");
                }
                else
                {
                    Debug.LogWarning("Ball not found!");
                }
                Destroy(gameObject);
            }
            else if (powerUpType == PowerUpType.Magnet)
            {
                Ball ballController = FindFirstObjectByType<Ball>();
                if (ballController != null)
                {
                    ballController.ApplyMagnet();
                    Debug.Log("Power-up applied: Magnet - Ball will stick to player");
                }
                else
                {
                    Debug.LogWarning("Ball not found!");
                }
                Destroy(gameObject);
            }
        }
    }
}