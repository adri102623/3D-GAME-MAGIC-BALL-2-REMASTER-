using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("Score Settings")]
    private int scorePerHit = 5; // Puntos por cada hit a un pickup
    private int scorePerPowerUp = 25; // Puntos por cada PowerUp recogida
    
    [Header("Lives Settings")]
    private int maxLives = 3; // Vidas máximas
    private int currentLives = 3; // Vidas actuales
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText; // Texto para mostrar vidas
    private int currentScore = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ScoreManager: Instance created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("ScoreManager: Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"ScoreManager: Start called in scene '{currentScene}'");
        
        // Solo inicializar UI si estamos en un nivel de juego
        if (IsGameLevel(currentScene))
        {
            Debug.Log("ScoreManager: Initializing UI for game level");
            InitializeUI();
            UpdateScoreUI();
            UpdateLivesUI();
        }
        else
        {
            Debug.Log($"ScoreManager: Skipping UI initialization in non-game scene: {currentScene}");
        }
    }
    
    void InitializeUI()
    {
        if (scoreText == null)
        {
            FindOrCreateScoreText();
        }
        if (livesText == null)
        {
            FindOrCreateLivesText();
        }
    }
    
    void FindOrCreateScoreText()
    {
        // Primero buscar si ya existe
        GameObject scoreObject = GameObject.FindGameObjectWithTag("ScoreText");
        if (scoreObject != null)
        {
            scoreText = scoreObject.GetComponent<TextMeshProUGUI>();
            Debug.Log("ScoreText found in scene");
        }
        else
        {
            // Si no existe, crear desde prefab solo en niveles de juego
            if (IsGameLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            {
                CreateScoreTextFromPrefab();
            }
        }
    }
    
    void FindOrCreateLivesText()
    {
        // Primero buscar si ya existe
        GameObject livesObject = GameObject.FindGameObjectWithTag("LivesText");
        if (livesObject != null)
        {
            livesText = livesObject.GetComponent<TextMeshProUGUI>();
            Debug.Log("LivesText found in scene");
        }
        else
        {
            // Si no existe, crear desde prefab solo en niveles de juego
            if (IsGameLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            {
                CreateLivesTextFromPrefab();
            }
        }
    }
    
    void CreateScoreTextFromPrefab()
    {
        GameObject scoreTextPrefab = Resources.Load<GameObject>("Prefabs/ScoreText");
        if (scoreTextPrefab != null)
        {
            // Buscar Canvas en la escena
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                GameObject scoreInstance = Instantiate(scoreTextPrefab, canvas.transform);
                scoreText = scoreInstance.GetComponent<TextMeshProUGUI>();
                Debug.Log("ScoreText created from prefab");
            }
            else
            {
                Debug.LogWarning("No Canvas found in scene for ScoreText!");
            }
        }
        else
        {
            Debug.LogWarning("ScoreText prefab not found in Resources/Prefabs/!");
        }
    }
    
    void CreateLivesTextFromPrefab()
    {
        GameObject livesTextPrefab = Resources.Load<GameObject>("Prefabs/LivesText");
        if (livesTextPrefab != null)
        {
            // Buscar Canvas en la escena
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                GameObject livesInstance = Instantiate(livesTextPrefab, canvas.transform);
                livesText = livesInstance.GetComponent<TextMeshProUGUI>();
                Debug.Log("LivesText created from prefab");
            }
            else
            {
                Debug.LogWarning("No Canvas found in scene for LivesText!");
            }
        }
        else
        {
            Debug.LogWarning("LivesText prefab not found in Resources/Prefabs/!");
        }
    }
    
    // Incrementar por hit a pickup
    public void IncrementScore()
    {
        currentScore += scorePerHit;
        UpdateScoreUI();
        Debug.Log($"Score increased by hit! Current score: {currentScore}");
    }
    
    // Incrementar por PowerUp
    public void IncrementScoreForPowerUp()
    {
        currentScore += scorePerPowerUp;
        UpdateScoreUI();
        Debug.Log($"Score increased by PowerUp (+{scorePerPowerUp})! Current score: {currentScore}");
    }
    
    // Incrementar por cantidad personalizada
    public void IncrementScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        Debug.Log($"Score increased by {points}! Current score: {currentScore}");
    }
    
    // Métodos para gestión de vidas
    public void LoseLife()
    {
        currentLives--;
        UpdateLivesUI();
        Debug.Log($"Life lost! Remaining lives: {currentLives}");
        
        if (currentLives <= 0)
        {
            // Game Over - ir a GameOver scene
            Debug.Log("Game Over! No more lives remaining. Going to GameOver scene.");
            OnGameOver();
        }
        else
        {
            // Recargar nivel actual
            Debug.Log("Reloading current level...");
            ReloadCurrentLevel();
        }
    }
    
    public void ResetLives()
    {
        currentLives = maxLives;
        UpdateLivesUI();
        Debug.Log($"Lives reset to {maxLives}");
    }
    
    public int GetCurrentLives()
    {
        return currentLives;
    }
    
    private void OnGameOver()
{
    // Guardar high score si es necesario
    int highScore = HighScoreManager.LoadHighScore();
    if (currentScore > highScore)
    {
        HighScoreManager.SaveHighScore(currentScore);
        Debug.Log("¡Nuevo récord! " + currentScore);
    }

    // NO resetear aquí, solo destruir UI y ir a GameOver
    Debug.Log("Game Over - Going to GameOver scene");
    
    // Destruir UI de juego antes de cargar GameOver
    DestroyGameUI();
    
    // Cargar escena GameOver (mantener el comportamiento original)
    UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
}
    
    private void ReloadCurrentLevel()
    {
        // Resetear BallManager antes de recargar nivel
        if (BallManager.Instance != null)
        {
            BallManager.Instance.ResetBallManager();
        }
        
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
        Debug.Log("Score reset to 0");
    }
    
    void UpdateScoreUI()
    {
        // Solo actualizar UI si estamos en un nivel de juego
        if (!IsGameLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
        {
            return;
        }
        
        if (scoreText == null)
        {
            FindOrCreateScoreText();
        }
        
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }
    
    void UpdateLivesUI()
    {
        // Solo actualizar UI si estamos en un nivel de juego
        if (!IsGameLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
        {
            return;
        }
        
        if (livesText == null)
        {
            FindOrCreateLivesText();
        }
        
        if (livesText != null)
        {
            livesText.text = "Lives: " + currentLives.ToString();
        }
    }
    
    // Método para resetear TODO cuando se entra al menú
    public void OnMenuEntered()
    {
        ResetScore();  // Resetear score SIEMPRE al entrar al menú
        ResetLives();  // Resetear vidas SIEMPRE al entrar al menú
        Debug.Log("Entered menu - score and lives reset");
        
        // Destruir UI de juego cuando se entra al menú
        DestroyGameUI();
    }
    
    private void DestroyGameUI()
    {
        // Destruir ScoreText si existe
        if (scoreText != null)
        {
            if (scoreText.gameObject != null)
            {
                Destroy(scoreText.gameObject);
            }
            scoreText = null;
        }
        
        // Destruir LivesText si existe
        if (livesText != null)
        {
            if (livesText.gameObject != null)
            {
                Destroy(livesText.gameObject);
            }
            livesText = null;
        }
        
        Debug.Log("Game UI destroyed");
    }
    
    // Verificar si la escena es un nivel de juego
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
    
    public void OnSceneLoaded()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"ScoreManager: Scene loaded - {currentScene}");
        
        // Comportamiento según la escena
        if (IsGameLevel(currentScene))
        {
            // En niveles de juego: crear UI y actualizar
            Debug.Log("Game level detected - initializing UI");
            InitializeUI();
            UpdateScoreUI();
            UpdateLivesUI();
        }
        else if (currentScene == "Menu")
        {
            // En menú: resetear TODO y destruir UI (desde cualquier lugar)
            OnMenuEntered();
        }
        else if (currentScene == "GameOver")
        {
            // En GameOver: solo destruir UI, NO resetear datos
            DestroyGameUI();
        }
        else if (currentScene == "Credtis") // Credits
        {
            // En créditos: destruir UI
            DestroyGameUI();
        }
    }
}