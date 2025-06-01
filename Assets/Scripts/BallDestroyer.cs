using UnityEngine;

public class BallDestroyer : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float destroyZ = -45f;
    [SerializeField] private float destroyXPositive = 60f;
    [SerializeField] private float destroyXNegative = -60f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;

    private Ball ballComponent;

    void Start()
    {
        ballComponent = GetComponent<Ball>();
        if (ballComponent == null)
        {
            Debug.LogError("BallDestroyer: No Ball component found on this GameObject!");
        }
    }

    void Update()
    {
        if (ballComponent == null) return;

        Vector3 position = transform.position;
        
        // Verificar límites
        if (position.z <= destroyZ || position.x >= destroyXPositive || position.x <= destroyXNegative)
        {
            string boundaryType = "";
            if (position.z <= destroyZ) boundaryType = "Z";
            else if (position.x >= destroyXPositive) boundaryType = "X+";
            else if (position.x <= destroyXNegative) boundaryType = "X-";

            if (showDebugInfo) Debug.Log($"Ball reached {boundaryType} boundary at {position}. Self-destroying...");
            
            // Notificar al BallManager antes de destruirse
            BallManager.Instance?.OnBallDestroyed(ballComponent);
            
            Destroy(gameObject);
        }
    }
}
