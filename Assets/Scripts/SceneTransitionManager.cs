using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
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
    }
    
    public void LoadNextLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadNextLevel(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}