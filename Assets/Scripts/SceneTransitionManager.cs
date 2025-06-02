using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Level Settings")]
    public string[] levelNames = { "lvl1", "lvl2", "lvl3", "lvl4", "lvl5" };
    public string menuSceneName = "Menu";
    public string creditsSceneName = "Credtis";
    public string MaxScoreSceneName = "MaxScore";
    public string gameOverSceneName = "GameOver";

    private int currentLevelIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // NUEVO: Desactivar componentes que no se necesitan en Menu
            string currentScene = SceneManager.GetActiveScene().name;
            ConfigureManagerForScene(currentScene);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // NUEVO: Configurar qué componentes están activos según la escena
    void ConfigureManagerForScene(string sceneName)
    {
        // Obtener componentes del GameManager
        BallBoundaryChecker boundaryChecker = GetComponent<BallBoundaryChecker>();
        BallManager ballManager = GetComponent<BallManager>();
        
        bool isGameLevel = IsGameLevel(sceneName);
        
        // En Menu: desactivar componentes de juego
        if (boundaryChecker != null)
        {
            boundaryChecker.enabled = isGameLevel;
        }
        
        if (ballManager != null)
        {
            ballManager.enabled = isGameLevel;
        }
        
        Debug.Log($"GameManager configured for scene '{sceneName}' - Game components enabled: {isGameLevel}");
    }

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusicForScene(currentScene);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) LoadLevel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) LoadLevel(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) LoadLevel(4);
        if (Input.GetKeyDown(KeyCode.M)) LoadMenu();
        if (Input.GetKeyDown(KeyCode.C)) LoadCredits();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SceneTransitionManager: Scene loaded: {scene.name}");
        
        // NUEVO: Reconfigurar componentes para la nueva escena
        ConfigureManagerForScene(scene.name);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusicForScene(scene.name);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnSceneLoaded();
        }

        if (IsGameLevel(scene.name))
        {
            StartLevelPresentation();

            // MODIFICADO: Solo verificar BallBoundaryChecker si está activo
            BallBoundaryChecker boundaryChecker = FindFirstObjectByType<BallBoundaryChecker>();
            if (boundaryChecker == null)
            {
                Debug.LogError($"CRITICAL ERROR: BallBoundaryChecker was NOT FOUND in game level scene '{scene.name}'. " +
                               "It MUST be manually placed in each level scene for the game to work correctly in builds.");
            }
            else
            {
                Debug.Log($"SceneTransitionManager: Found BallBoundaryChecker in '{scene.name}'. Calling ResetBoundaryChecker().");
                boundaryChecker.ResetBoundaryChecker();
                
                // NUEVO: Asegurar que esté habilitado en niveles de juego
                boundaryChecker.enabled = true;
            }

            // AÑADIR: Verificar que existe LevelProgressManager
            LevelProgressManager progressManager = FindFirstObjectByType<LevelProgressManager>();
            if (progressManager == null)
            {
                // Crear LevelProgressManager si no existe
                GameObject progressObject = new GameObject("LevelProgressManager");
                progressObject.AddComponent<LevelProgressManager>();
                Debug.Log("SceneTransitionManager: Created LevelProgressManager for progress tracking.");
            }
            else
            {
                Debug.Log("SceneTransitionManager: Found existing LevelProgressManager.");
            }
            
            // NUEVO: Asegurar que BallManager esté activo y reseteado
            BallManager ballManager = BallManager.Instance;
            if (ballManager != null)
            {
                ballManager.enabled = true;
                ballManager.ResetBallManager();
                Debug.Log("BallManager activated and reset for game level");
            }
        }
        else
        {
            // En escenas no-juego, desactivar componentes de juego
            BallBoundaryChecker boundaryChecker = GetComponent<BallBoundaryChecker>();
            BallManager ballManager = GetComponent<BallManager>();
            
            if (boundaryChecker != null) boundaryChecker.enabled = false;
            if (ballManager != null) ballManager.enabled = false;
            
            Debug.Log($"Game components disabled for non-game scene: {scene.name}");
        }
    }

    public void StartFirstLevel()
    {
        currentLevelIndex = 0;
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelNames.Length)
        {
            currentLevelIndex = levelIndex;
            Debug.Log($"SceneTransitionManager: Loading level: {levelNames[levelIndex]} (Index: {levelIndex})");
            SceneManager.LoadScene(levelNames[levelIndex]);
        }
        else
        {
            Debug.LogWarning($"SceneTransitionManager: Level index {levelIndex} is out of range!");
        }
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;

        // MODIFICADO: Si estamos en el último nivel (lvl5), ir a créditos
        if (currentLevelIndex >= levelNames.Length)
        {
            Debug.Log("SceneTransitionManager: All levels completed. Loading Credits.");
            LoadCredits();
        }
        else
        {
            LoadLevel(currentLevelIndex);
        }
    }

    public void LoadMenu()
    {
        Debug.Log("SceneTransitionManager: Loading Menu scene: " + menuSceneName);
        SceneManager.LoadScene(menuSceneName);
    }

    public void LoadCredits()
    {
        Debug.Log("SceneTransitionManager: Loading Credits scene: " + creditsSceneName);
        SceneManager.LoadScene(creditsSceneName);
    }

    public void LoadMaxScore()
    {
        Debug.Log("SceneTransitionManager: Loading MaxScore scene: " + MaxScoreSceneName);
        SceneManager.LoadScene(MaxScoreSceneName);
    }

    public void LoadGameOver()
    {
        Debug.Log("SceneTransitionManager: Loading GameOver scene: " + gameOverSceneName);
        SceneManager.LoadScene(gameOverSceneName);
    }

    private bool IsGameLevel(string sceneName)
    {
        foreach (string level in levelNames)
        {
            if (level == sceneName)
                return true;
        }
        return false;
    }

    private void StartLevelPresentation()
    {
        LevelPresentation presentation = FindFirstObjectByType<LevelPresentation>();
        if (presentation != null)
        {
            Debug.Log("SceneTransitionManager: Starting level presentation.");
            presentation.StartPresentation();
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager: LevelPresentation component not found in the scene for StartLevelPresentation call!");
        }
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }

    public string GetCurrentLevelName()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levelNames.Length)
        {
            return levelNames[currentLevelIndex];
        }
        Debug.LogWarning("SceneTransitionManager: GetCurrentLevelName called with invalid currentLevelIndex: " + currentLevelIndex);
        return string.Empty;
    }
}