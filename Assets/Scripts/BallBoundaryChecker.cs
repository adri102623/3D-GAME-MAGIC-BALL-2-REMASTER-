using UnityEngine;
using System.Collections;

public class BallBoundaryChecker : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float loseLifeZ = -45f;
    [SerializeField] private float loseLifeXPositive = 60f;
    [SerializeField] private float loseLifeXNegative = -60f;

    [Header("Debug")]
    public bool showDebugInfo = true; 

    private bool hasLostLife = false;
    private float checkInterval = 0.2f; 

    void OnEnable()
    {
        if (showDebugInfo) Debug.Log($"BallBoundaryChecker: ENABLED in scene '{gameObject.scene.name}'. Initializing state...");
        hasLostLife = false;
        StopAllCoroutines(); 
        StartCoroutine(PeriodicCheckRoutine());
    }

    void OnDisable()
    {
        if (showDebugInfo) Debug.Log($"BallBoundaryChecker: DISABLED in scene '{gameObject.scene.name}'. Stopping coroutines.");
        StopAllCoroutines();
    }

    private IEnumerator PeriodicCheckRoutine()
    {
        if (showDebugInfo) Debug.Log("BallBoundaryChecker: Starting periodic check routine...");
        yield return null; 

        while (true) 
        {
            // Buscar todas las bolas en la escena
            Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
            
            if (balls.Length == 0)
            {
                if (showDebugInfo) Debug.Log("BallBoundaryChecker: No balls found in scene.");
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // Verificar cada bola y eliminar las que pasen los límites
            bool anyBallRemoved = false;
            foreach (Ball ball in balls)
            {
                if (ball == null) continue;

                Vector3 ballPos = ball.transform.position;
                
                // Verificar límites Z y X
                if (ballPos.z <= loseLifeZ || ballPos.x >= loseLifeXPositive || ballPos.x <= loseLifeXNegative)
                {
                    string boundaryType = "";
                    if (ballPos.z <= loseLifeZ) boundaryType = "Z";
                    else if (ballPos.x >= loseLifeXPositive) boundaryType = "X+";
                    else if (ballPos.x <= loseLifeXNegative) boundaryType = "X-";

                    if (showDebugInfo) Debug.Log($"BallBoundaryChecker: Ball passed {boundaryType} boundary at {ballPos}. Removing ball...");
                    
                    // Eliminar la bola
                    Destroy(ball.gameObject);
                    anyBallRemoved = true;
                }
            }

            // Si se eliminó alguna bola, verificar si quedan bolas después de la eliminación
            if (anyBallRemoved)
            {
                // Esperar un frame para que se complete la destrucción
                yield return new WaitForFixedUpdate();
                
                // Verificar cuántas bolas quedan
                Ball[] remainingBalls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
                int validBallCount = 0;
                
                foreach (Ball ball in remainingBalls)
                {
                    if (ball != null && ball.gameObject != null) validBallCount++;
                }
                
                if (showDebugInfo) Debug.Log($"BallBoundaryChecker: {validBallCount} balls remaining after boundary check.");
                
                // Si no quedan bolas válidas, perder vida
                if (validBallCount == 0 && !hasLostLife)
                {
                    LoseLifeForNoBalls();
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void LoseLifeForNoBalls()
    {
        if (hasLostLife) return;

        hasLostLife = true;
        Debug.Log("BallBoundaryChecker: No balls remaining in field. Losing a life...");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.LoseLife();
        }
        else
        {
            Debug.LogError("BallBoundaryChecker: ScoreManager.Instance is NULL when trying to lose life!");
        }
    }

    public void ResetBoundaryChecker()
    {
        if (showDebugInfo) Debug.Log("BallBoundaryChecker: ResetBoundaryChecker called. Resetting hasLostLife state.");
        hasLostLife = false;
        StopAllCoroutines();
        StartCoroutine(PeriodicCheckRoutine());
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo && !Application.isEditor) return; 

        Gizmos.color = hasLostLife ? Color.gray : Color.red; 
        
        // LÍNEA Z (original)
        Vector3 centerLineZ = new Vector3(transform.position.x, transform.position.y, loseLifeZ);
        Vector3 sizeLineZ = new Vector3(100, 0.2f, 0.2f); 
        Gizmos.DrawCube(centerLineZ, sizeLineZ);

        // LÍNEAS X
        Gizmos.color = hasLostLife ? Color.gray : Color.blue; 
        
        // Línea X positiva
        Vector3 centerLineXPos = new Vector3(loseLifeXPositive, transform.position.y, transform.position.z);
        Vector3 sizeLineX = new Vector3(0.2f, 0.2f, 100); 
        Gizmos.DrawCube(centerLineXPos, sizeLineX);
        
        // Línea X negativa
        Vector3 centerLineXNeg = new Vector3(loseLifeXNegative, transform.position.y, transform.position.z);
        Gizmos.DrawCube(centerLineXNeg, sizeLineX);

        #if UNITY_EDITOR
        // Labels Z
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.Label(centerLineZ + Vector3.up * 1.5f, $"Life Boundary Z: {loseLifeZ}");
        
        // Labels X
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(centerLineXPos + Vector3.up * 1.5f, $"Life Boundary X+: {loseLifeXPositive}");
        UnityEditor.Handles.Label(centerLineXNeg + Vector3.up * 1.5f, $"Life Boundary X-: {loseLifeXNegative}");
        
        // Ball count
        Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"Balls in scene: {balls.Length}");
        #endif
    }
}