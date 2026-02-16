using UnityEngine;
using TMPro; // Needs TextMeshPro
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Easy access for other scripts

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;

    [Header("Game Settings")]
    public float timeRemaining = 60f;
    private int _score = 0;
    private bool _isGameActive = true;

    void Awake() { Instance = this; }

    void Update()
    {
        if (_isGameActive)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                EndGame();
            }
        }
    }

    public void AddScore(int points)
    {
        if (!_isGameActive) return;
        _score += points;
        scoreText.text = "Sparks Sorted: " + _score;
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void EndGame()
    {
        _isGameActive = false;
        gameOverPanel.SetActive(true);
        // Here you would send the final _score to the Teacher Dashboard
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
