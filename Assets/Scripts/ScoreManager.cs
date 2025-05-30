using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("Score Settings")]
    private int scorePerHit = 5; // Puntos por cada hit a un pickup
    private int scorePerPowerUp = 25; // Puntos por cada PowerUp recogida
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    
    private int currentScore = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateScoreUI();
    }
    
    void InitializeUI()
    {
        if (scoreText == null)
        {
            FindScoreText();
        }
    }
    
    void FindScoreText()
    {
        GameObject scoreObject = GameObject.FindGameObjectWithTag("ScoreText");
        if (scoreObject != null)
        {
            scoreText = scoreObject.GetComponent<TextMeshProUGUI>();
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
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }
    
    void UpdateScoreUI()
    {
        if (scoreText == null)
        {
            FindScoreText();
        }
        
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }
    
    public void OnSceneLoaded()
    {
        FindScoreText();
        UpdateScoreUI();
    }
}