using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public void StartGame()
    {
        // Usar SceneTransitionManager en lugar de cargar directamente
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.StartFirstLevel();
        }
        else
        {
            // Fallback si no existe el SceneTransitionManager
            SceneManager.LoadScene("lvl1");
        }
    }

    public void Return()
    {
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
        SceneManager.LoadScene("Credtis");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    // Métodos adicionales para testing de niveles específicos
    public void LoadLevel1() { SceneTransitionManager.Instance?.LoadLevel(0); }
    public void LoadLevel2() { SceneTransitionManager.Instance?.LoadLevel(1); }
    public void LoadLevel3() { SceneTransitionManager.Instance?.LoadLevel(2); }
    public void LoadLevel4() { SceneTransitionManager.Instance?.LoadLevel(3); }
    public void LoadLevel5() { SceneTransitionManager.Instance?.LoadLevel(4); }
}