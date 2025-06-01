using UnityEngine;
using TMPro;

public class MaxScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        int highScore = HighScoreManager.LoadHighScore();
        highScoreText.text = highScore.ToString();
    }
}   