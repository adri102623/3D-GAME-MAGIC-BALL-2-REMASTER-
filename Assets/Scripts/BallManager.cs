using UnityEngine;
using System.Collections;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance { get; private set; }
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private bool hasLostLife = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (showDebugInfo) Debug.Log("BallManager: Initialized");
        
        // Reset del flag al inicio
        hasLostLife = false;
    }

    public void OnBallDestroyed(Ball destroyedBall)
    {
        if (showDebugInfo) Debug.Log($"BallManager: Ball {destroyedBall.name} was destroyed");
        
        // Esperar un frame para verificar bolas restantes
        StartCoroutine(CheckRemainingBalls());
    }

    private IEnumerator CheckRemainingBalls()
    {
        yield return new WaitForFixedUpdate();
        
        Ball[] remainingBalls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        int validBallCount = 0;
        
        foreach (Ball ball in remainingBalls)
        {
            if (ball != null && ball.gameObject != null) validBallCount++;
        }
        
        if (showDebugInfo) Debug.Log($"BallManager: {validBallCount} balls remaining");
        
        // CLAVE: Solo perder vida si no quedan bolas Y no se ha perdido ya
        if (validBallCount == 0 && !hasLostLife)
        {
            LoseLifeForNoBalls();
        }
    }

    private void LoseLifeForNoBalls()
    {
        hasLostLife = true;
        Debug.Log("BallManager: No balls remaining in field. Losing a life...");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.LoseLife();
        }
        else
        {
            Debug.LogError("BallManager: ScoreManager.Instance is NULL!");
        }
    }

    // CLAVE: Método para resetear estado al cargar nivel
    public void ResetBallManager()
    {
        hasLostLife = false;
        if (showDebugInfo) Debug.Log("BallManager: Reset completed");
    }

    // CLAVE: Reset automático al habilitar el objeto
    void OnEnable()
    {
        hasLostLife = false;
        if (showDebugInfo) Debug.Log("BallManager: OnEnable - hasLostLife reset");
    }

    public int GetBallCount()
    {
        Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        int validCount = 0;
        foreach (Ball ball in balls)
        {
            if (ball != null && ball.gameObject != null) validCount++;
        }
        return validCount;
    }
}
