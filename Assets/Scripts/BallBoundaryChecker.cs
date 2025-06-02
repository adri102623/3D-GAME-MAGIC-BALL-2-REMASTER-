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

    private float checkInterval = 0.2f; 

    void OnEnable()
    {
        // Solo iniciar si estamos en un nivel de juego
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (IsGameLevel(currentScene))
        {
            if (showDebugInfo) Debug.Log("BallBoundaryChecker: OnEnable - Starting coroutine for game level");
            StartCoroutine(PeriodicCheckRoutine());
        }
        else
        {
            if (showDebugInfo) Debug.Log($"BallBoundaryChecker: OnEnable - Skipping start in non-game scene: {currentScene}");
        }
    }

    void OnDisable()
    {
        if (showDebugInfo) Debug.Log("BallBoundaryChecker: OnDisable - Stopping all coroutines");
        StopAllCoroutines();
    }

    private IEnumerator PeriodicCheckRoutine()
    {
        if (showDebugInfo) Debug.Log("BallBoundaryChecker: Starting periodic check routine...");
        yield return null; 

        while (true) 
        {
            Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
            
            if (balls.Length == 0)
            {
                if (showDebugInfo) Debug.Log("BallBoundaryChecker: No balls found in scene.");
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // CAMBIADO: Solo eliminar bolas, NO manejar vidas
            foreach (Ball ball in balls)
            {
                if (ball == null) continue;

                Vector3 ballPos = ball.transform.position;
                
                if (ballPos.z <= loseLifeZ || ballPos.x >= loseLifeXPositive || ballPos.x <= loseLifeXNegative)
                {
                    string boundaryType = "";
                    if (ballPos.z <= loseLifeZ) boundaryType = "Z";
                    else if (ballPos.x >= loseLifeXPositive) boundaryType = "X+";
                    else if (ballPos.x <= loseLifeXNegative) boundaryType = "X-";

                    if (showDebugInfo) Debug.Log($"BallBoundaryChecker: Ball passed {boundaryType} boundary at {ballPos}. Removing ball...");
                    
                    // SOLO eliminar - BallManager manejará la lógica de vidas
                    Destroy(ball.gameObject);
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // NUEVO: Verificar si es nivel de juego
    private bool IsGameLevel(string sceneName)
    {
        string[] levelNames = { "lvl1", "lvl2", "lvl3", "lvl4", "lvl5" };
        foreach (string levelName in levelNames)
        {
            if (sceneName == levelName)
                return true;
        }
        return false;
    }

    // MANTENIDO: Para compatibilidad con SceneTransitionManager
    public void ResetBoundaryChecker()
    {
        // Solo funcionar si estamos en un nivel de juego
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!IsGameLevel(currentScene))
        {
            if (showDebugInfo) Debug.Log($"BallBoundaryChecker: ResetBoundaryChecker called in non-game scene '{currentScene}' - ignoring");
            return;
        }
        
        if (showDebugInfo) Debug.Log("BallBoundaryChecker: ResetBoundaryChecker called. Restarting checks.");
        StopAllCoroutines();
        StartCoroutine(PeriodicCheckRoutine());
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo && !Application.isEditor) return; 

        Gizmos.color = Color.red; 
        
        Vector3 centerLineZ = new Vector3(transform.position.x, transform.position.y, loseLifeZ);
        Vector3 sizeLineZ = new Vector3(100, 0.2f, 0.2f); 
        Gizmos.DrawCube(centerLineZ, sizeLineZ);

        Gizmos.color = Color.blue; 
        
        Vector3 centerLineXPos = new Vector3(loseLifeXPositive, transform.position.y, transform.position.z);
        Vector3 sizeLineX = new Vector3(0.2f, 0.2f, 100); 
        Gizmos.DrawCube(centerLineXPos, sizeLineX);
        
        Vector3 centerLineXNeg = new Vector3(loseLifeXNegative, transform.position.y, transform.position.z);
        Gizmos.DrawCube(centerLineXNeg, sizeLineX);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.Label(centerLineZ + Vector3.up * 1.5f, $"Life Boundary Z: {loseLifeZ}");
        
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(centerLineXPos + Vector3.up * 1.5f, $"Life Boundary X+: {loseLifeXPositive}");
        UnityEditor.Handles.Label(centerLineXNeg + Vector3.up * 1.5f, $"Life Boundary X-: {loseLifeXNegative}");
        
        Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"Balls in scene: {balls.Length}");
        #endif
    }
}