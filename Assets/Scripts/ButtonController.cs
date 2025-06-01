using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("StartGame button pressed");
        
        // Usar SceneTransitionManager en lugar de cargar directamente
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.StartFirstLevel();
        }
        else
        {
            // Fallback si no existe el SceneTransitionManager
            Debug.Log("SceneTransitionManager not found, loading directly");
            SceneManager.LoadScene("lvl1");
        }
    }

    public void Return()
    {
        Debug.Log("Return button pressed");
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadMenu();
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    public void Credits()
    {
        Debug.Log("Credits button pressed");
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadCredits();
        }
        else
        {
            SceneManager.LoadScene("Credtis");
        }
    }
    
    public void MaxScore()
    {
        Debug.Log("MaxScore button pressed");
        
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadMaxScore();
        }
        else
        {
            SceneManager.LoadScene("MaxScore");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();

        // Para testing en el editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
    // Métodos adicionales para testing de niveles específicos
    public void LoadLevel1() 
    { 
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadLevel(0);
    }
    
    public void LoadLevel2() 
    { 
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadLevel(1);
    }
    
    public void LoadLevel3() 
    { 
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadLevel(2);
    }
    
    public void LoadLevel4() 
    { 
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadLevel(3);
    }
    
    public void LoadLevel5() 
    { 
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadLevel(4);
    }
}