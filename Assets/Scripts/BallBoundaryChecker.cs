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

    private Ball ball;
    private bool hasLostLife = false;
    private float checkInterval = 0.2f; 

    void OnEnable()
    {
        if (showDebugInfo) Debug.Log($"BallBoundaryChecker: ENABLED in scene '{gameObject.scene.name}'. Initializing state...");
        hasLostLife = false;
        ball = null; 
        StopAllCoroutines(); 
        StartCoroutine(InitializeAndPeriodicCheckRoutine());
    }

    void OnDisable()
    {
        if (showDebugInfo) Debug.Log($"BallBoundaryChecker: DISABLED in scene '{gameObject.scene.name}'. Stopping coroutines.");
        StopAllCoroutines();
    }

    private IEnumerator InitializeAndPeriodicCheckRoutine()
    {
        if (showDebugInfo) Debug.Log("BallBoundaryChecker: Coroutine InitializeAndPeriodicCheckRoutine STARTED.");
        yield return null; 

        int attempts = 0;
        int maxInitialAttempts = 50; 
        float initialRetryInterval = 0.1f;

        if (showDebugInfo) Debug.Log("BallBoundaryChecker: Starting INITIAL ball search phase...");
        while (ball == null && attempts < maxInitialAttempts)
        {
            ball = FindFirstObjectByType<Ball>(); 

            if (ball == null) 
            {
                GameObject ballGO = GameObject.FindGameObjectWithTag("Ball"); 
                if (ballGO != null)
                {
                    ball = ballGO.GetComponent<Ball>();
                    if (ball != null && showDebugInfo) Debug.Log("BallBoundaryChecker: Ball found via GameObject.FindGameObjectWithTag(\"Ball\")");
                }
            }

            if (ball == null)
            {
                if (showDebugInfo) Debug.Log($"BallBoundaryChecker: Ball not found (Initial Attempt {attempts + 1}/{maxInitialAttempts}). Retrying in {initialRetryInterval}s...");
                yield return new WaitForSeconds(initialRetryInterval);
                attempts++;
            }
            else
            {
                if (showDebugInfo) Debug.Log($"BallBoundaryChecker: Ball INITIALIZED and FOUND: {ball.name}. Current Position: X={ball.transform.position.x:F2}, Z={ball.transform.position.z:F2}");
                break; 
            }
        }

        if (ball == null)
        {
            Debug.LogError("BallBoundaryChecker: CRITICAL - Ball NOT FOUND after initial intensive search. Periodic checks will continue trying, but this may indicate a problem.");
        }

        if (showDebugInfo) Debug.Log("BallBoundaryChecker: Starting PERIODIC check phase...");
        while (true) 
        {
            if (ball == null)
            {
                ball = FindFirstObjectByType<Ball>();
                 if (ball == null)
                {
                    GameObject ballGO = GameObject.FindGameObjectWithTag("Ball");
                    if (ballGO != null) ball = ballGO.GetComponent<Ball>();
                }

                if (ball != null)
                {
                    if (showDebugInfo) Debug.Log("BallBoundaryChecker: Ball (re)acquired in periodic check.");
                }
                else
                {
                    if (showDebugInfo) Debug.Log("BallBoundaryChecker: Ball still null in periodic check. Waiting for next interval.");
                    yield return new WaitForSeconds(checkInterval * 2f); 
                    continue; 
                }
            }

            // MODIFICADO: Verificar tanto Z como X
            Vector3 ballPos = ball.transform.position;
            bool isNearBoundary = ballPos.z < (loseLifeZ + 10f) || 
                                  ballPos.x > (loseLifeXPositive - 10f) || 
                                  ballPos.x < (loseLifeXNegative + 10f);

            if (showDebugInfo && isNearBoundary)
            {
                Debug.Log($"BallBoundaryChecker: Periodic Check - Ball Position: X={ballPos.x:F2}, Z={ballPos.z:F2}, Boundaries: Z<={loseLifeZ}, X>={loseLifeXPositive} or X<={loseLifeXNegative}, HasLostLife: {hasLostLife}");
            }

            // MODIFICADO: Verificar límites Z y X
            if (!hasLostLife && (ballPos.z <= loseLifeZ || ballPos.x >= loseLifeXPositive || ballPos.x <= loseLifeXNegative))
            {
                string boundaryType = "";
                if (ballPos.z <= loseLifeZ) boundaryType = "Z";
                else if (ballPos.x >= loseLifeXPositive) boundaryType = "X+";
                else if (ballPos.x <= loseLifeXNegative) boundaryType = "X-";

                if (showDebugInfo) Debug.Log($"BallBoundaryChecker: {boundaryType} boundary condition MET. Calling OnBallPassedBoundaryInternal.");
                OnBallPassedBoundaryInternal(boundaryType);
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // MODIFICADO: Añadir parámetro para tipo de límite
    private void OnBallPassedBoundaryInternal(string boundaryType = "Z")
    {
        if (hasLostLife) 
        {
            if (showDebugInfo) Debug.Log("BallBoundaryChecker: OnBallPassedBoundaryInternal called, but hasLostLife is already true. Ignoring.");
            return;
        }

        hasLostLife = true; 
        string ballPos = (ball != null) ? $"X={ball.transform.position.x:F2}, Z={ball.transform.position.z:F2}" : "N/A (ball reference was null)";
        Debug.Log($"BallBoundaryChecker: Ball PASSED {boundaryType} BOUNDARY at {ballPos}. Losing a life...");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.LoseLife();
        }
        else
        {
            Debug.LogError("BallBoundaryChecker: ScoreManager.Instance is NULL when trying to lose life! This is a critical issue.");
        }
    }

    public void ResetBoundaryChecker()
    {
        if (showDebugInfo) Debug.Log("BallBoundaryChecker: ResetBoundaryChecker called. Resetting hasLostLife and ball reference. Restarting main coroutine.");
        hasLostLife = false;
        ball = null; 

        StopAllCoroutines();
        StartCoroutine(InitializeAndPeriodicCheckRoutine());
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
        
        // Ball position
        if (ball != null)
        {
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(ball.transform.position + Vector3.up * 2f, $"Ball: X={ball.transform.position.x:F2}, Z={ball.transform.position.z:F2}");
        }
        #endif
    }
}