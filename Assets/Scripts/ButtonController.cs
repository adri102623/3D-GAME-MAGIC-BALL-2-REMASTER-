using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        SceneManager.LoadScene("lvl1");
    }

    public void Credits()
    {
        SceneManager.LoadScene("credits");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
