using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Essential for switching levels
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel; // The panel that appears at the end
    public Button restartButton;
    public Button hubButton;

    [Header("Settings")]
    public float timeRemaining = 60f;
    private int _score = 0;
    private bool _isGameActive = true;

    void Awake() { Instance = this; }

    void Start()
    {
        gameOverPanel.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        // Link the buttons via code (Cleaner for Unity 6)
        restartButton.onClick.AddListener(RestartGame);
        hubButton.onClick.AddListener(GoToHub);
    }

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
        if (_score == 0) scoreText.gameObject.SetActive(true);
        _score += points;
        scoreText.text = _score.ToString();
    }

    void DisplayTime(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void EndGame()
    {
        _isGameActive = false;
        timeRemaining = 0;
        gameOverPanel.SetActive(true);
        
        // STOP ALL MOVEMENT: Freeze physics for all sparks
        Time.timeScale = 0; // This pauses the physics engine
    }

    public void RestartGame()
    {
        Time.timeScale = 1; // RESET time before reloading!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHub()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene("HubWorld"); // Ensure your Hub scene is named this
    }
}

