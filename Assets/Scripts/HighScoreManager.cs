using System.IO;
using UnityEngine;

public static class HighScoreManager
{
    private static string filePath = Application.persistentDataPath + "/highscore.txt";

    public static int LoadHighScore()
    {
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            int score;
            if (int.TryParse(content, out score))
                return score;
        }
        return 0;
    }

    public static void SaveHighScore(int score)
    {
        File.WriteAllText(filePath, score.ToString());
    }
}