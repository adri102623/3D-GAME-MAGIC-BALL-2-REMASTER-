using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("Level Settings")]
    public string[] levelNames = { "lvl1", "lvl2", "lvl3", "lvl4", "lvl5" };
    public string menuSceneName = "Menu";
    
    private int currentLevelIndex = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // Teclas para acceso rápido a niveles (solo para testing)
        if (Input.GetKeyDown(KeyCode.Alpha1)) LoadLevel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) LoadLevel(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) LoadLevel(4);
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Notificar al ScoreManager que se cargó una nueva escena
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnSceneLoaded();
        }
        
        // Iniciar presentación del nivel si es un nivel de juego
        if (IsGameLevel(scene.name))
        {
            StartLevelPresentation();
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
            SceneManager.LoadScene(levelNames[levelIndex]);
        }
        else
        {
            Debug.LogWarning($"Level index {levelIndex} is out of range!");
        }
    }
    
    public void LoadNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < levelNames.Length)
        {
            LoadLevel(nextLevelIndex);
        }
        else
        {
            // Último nivel completado, volver al menú o mostrar créditos
            LoadMenu();
        }
    }
    
    public void LoadMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
    
    public void LoadNextLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadNextLevel(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
    private bool IsGameLevel(string sceneName)
    {
        foreach (string levelName in levelNames)
        {
            if (sceneName == levelName)
                return true;
        }
        return false;
    }
    
    private void StartLevelPresentation()
    {
        // Buscar el componente LevelPresentation en la escena
        LevelPresentation presentation = FindFirstObjectByType<LevelPresentation>();
        if (presentation != null)
        {
            presentation.StartPresentation();
        }
        else
        {
            Debug.LogWarning("LevelPresentation component not found in the scene!");
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
        return "";
    }
}