using UnityEngine;
using System.Collections;

public class BallBoundaryChecker : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float loseLifeZ = -45f;

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
                if (showDebugInfo) Debug.Log($"BallBoundaryChecker: Ball INITIALIZED and FOUND: {ball.name}. Current Z: {ball.transform.position.z:F2}");
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

            if (showDebugInfo)
            {
                if (ball.transform.position.z < (loseLifeZ + 10f)) {
                     Debug.Log($"BallBoundaryChecker: Periodic Check - Ball Z: {ball.transform.position.z:F2}, Boundary: {loseLifeZ}, HasLostLife: {hasLostLife}");
                }
            }

            if (ball.transform.position.z <= loseLifeZ && !hasLostLife)
            {
                if (showDebugInfo) Debug.Log("BallBoundaryChecker: Boundary condition MET. Calling OnBallPassedBoundaryInternal.");
                OnBallPassedBoundaryInternal();
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void OnBallPassedBoundaryInternal()
    {
        if (hasLostLife) 
        {
            if (showDebugInfo) Debug.Log("BallBoundaryChecker: OnBallPassedBoundaryInternal called, but hasLostLife is already true. Ignoring.");
            return;
        }

        hasLostLife = true; 
        string ballZPos = (ball != null) ? ball.transform.position.z.ToString("F2") : "N/A (ball reference was null)";
        Debug.Log($"BallBoundaryChecker: Ball PASSED BOUNDARY at Z={ballZPos}. Losing a life...");

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
        Vector3 centerLine = new Vector3(transform.position.x, transform.position.y, loseLifeZ);
        Vector3 sizeLine = new Vector3(100, 0.2f, 0.2f); 
        Gizmos.DrawCube(centerLine, sizeLine);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.Label(centerLine + Vector3.up * 1.5f, $"Life Boundary Z: {loseLifeZ}");
        if (ball != null)
        {
            UnityEditor.Handles.Label(ball.transform.position + Vector3.up * 2f, $"Ball Z: {ball.transform.position.z:F2}");
        }
        #endif
    }
}