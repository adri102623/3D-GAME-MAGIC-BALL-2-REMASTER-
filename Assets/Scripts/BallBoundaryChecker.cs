using UnityEngine;

public class BallBoundaryChecker : MonoBehaviour
{
    [Header("Boundary Settings")]
    private float loseLifeZ = -45f; // Posición Z donde se pierde una vida
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Ball ball;
    private bool hasLostLife = false; // Para evitar múltiples pérdidas de vida
    
    void Start()
    {
        // Buscar la pelota en la escena
        ball = FindFirstObjectByType<Ball>();
        if (ball == null)
        {
            Debug.LogWarning("Ball not found in scene!");
        }
        else
        {
            Debug.Log("BallBoundaryChecker initialized - watching ball at Z boundary: " + loseLifeZ);
        }
    }
    
    void Update()
    {
        if (ball == null) return;
        
        // Verificar si la pelota ha pasado el límite
        if (ball.transform.position.z <= loseLifeZ && !hasLostLife)
        {
            hasLostLife = true;
            OnBallPassedBoundary();
        }
        
        // Debug info
        if (showDebugInfo && ball.transform.position.z < -30f)
        {
            Debug.Log($"Ball Z position: {ball.transform.position.z:F2} (Boundary: {loseLifeZ})");
        }
    }
    
    void OnBallPassedBoundary()
    {
        Debug.Log($"Ball passed boundary at Z = {ball.transform.position.z:F2}! Losing a life...");
        
        // Notificar al ScoreManager para perder una vida
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.LoseLife();
        }
        else
        {
            Debug.LogError("ScoreManager not found!");
        }
    }
    
    // Método para resetear el estado (útil si la pelota se reinicia)
    public void ResetBoundaryChecker()
    {
        hasLostLife = false;
        Debug.Log("BallBoundaryChecker reset");
    }
    
    // Visualizar el límite en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, 0, loseLifeZ), new Vector3(50, 0, loseLifeZ));
        Gizmos.DrawLine(new Vector3(-50, 10, loseLifeZ), new Vector3(50, 10, loseLifeZ));
    }
}