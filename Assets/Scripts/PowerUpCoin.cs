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
        PowerBall,
        NextLevel // NUEVO
    }
    
    public PowerUpType powerUpType;
    public float speed = 5f;
    
    [Header("Rotación")]
    public float rotationSpeed = 180f;
    public Vector3 rotationAxis = Vector3.up;
    
    private float targetZ;
    private bool hasTarget = false;
    private bool collected = false;

    public void SetTargetZ(float zPosition)
    {
        targetZ = zPosition;
        hasTarget = true;
    }

    void Update()
    {
        if (!hasTarget || collected) return;

        Vector3 newPosition = transform.position;
        newPosition.z -= speed * Time.deltaTime;
        transform.position = newPosition;

        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        if (transform.position.z < targetZ - 5f)
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
            Debug.LogWarning("PowerUpCoin: ScoreManager not found! Make sure ScoreManager is in the scene.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player") || other.CompareTag("FrontTrigger"))
        {
            collected = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUpSound();
            }
            IncrementScoreForPowerUp();
            
            Ball ballController = FindFirstObjectByType<Ball>();
            Player playerController = FindFirstObjectByType<Player>();

            Debug.Log($"PowerUpCoin: '{powerUpType}' collected by '{other.gameObject.name}'.");

            switch (powerUpType)
            {
                case PowerUpType.Maximize:
                    if (ballController != null) ballController.ApplyPowerUp();
                    else Debug.LogWarning("PowerUpCoin: Ball not found for Maximize!");
                    break;
                case PowerUpType.PowerBall:
                    if (ballController != null) ballController.ApplyPowerUp_PowerBall();
                    else Debug.LogWarning("PowerUpCoin: Ball not found for PowerBall!");
                    break;
                case PowerUpType.ScaleBarrier:
                    if (playerController != null) playerController.ApplyPowerUp_MaxBarrier();
                    else Debug.LogWarning("PowerUpCoin: Player not found for ScaleBarrier!");
                    break;
                case PowerUpType.UnScaleBarrier:
                    if (playerController != null) playerController.ApplyPowerUp_MinBarrier();
                    else Debug.LogWarning("PowerUpCoin: Player not found for UnScaleBarrier!");
                    break;
                case PowerUpType.SpeedUp:
                    if (ballController != null) ballController.ApplySpeedUp();
                    else Debug.LogWarning("PowerUpCoin: Ball not found for SpeedUp!");
                    break;
                case PowerUpType.SpeedDown:
                    if (ballController != null) ballController.ApplySpeedDown();
                    else Debug.LogWarning("PowerUpCoin: Ball not found for SpeedDown!");
                    break;
                case PowerUpType.Magnet:
                    if (ballController != null) ballController.ApplyMagnet();
                    else Debug.LogWarning("PowerUpCoin: Ball not found for Magnet!");
                    break;
                case PowerUpType.NextLevel: // NUEVO
                    if (SceneTransitionManager.Instance != null)
                    {
                        Debug.Log("PowerUpCoin: NextLevel collected! Advancing to next level...");
                        SceneTransitionManager.Instance.LoadNextLevel();
                    }
                    else
                    {
                        Debug.LogWarning("PowerUpCoin: SceneTransitionManager not found for NextLevel!");
                    }
                    break;
                default:
                    Debug.LogWarning("PowerUpCoin: Unknown PowerUpType!");
                    break;
            }
            Destroy(gameObject);
        }
    }
}