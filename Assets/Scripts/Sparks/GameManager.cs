using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button hubButton;

    [Header("Settings")]
    private float _timeElapsed = 0f;
    private int _score = 0;
    private bool _isGameActive = true;

    void Awake() { Instance = this; }

    void Start()
    {
        gameOverPanel.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        else Debug.LogWarning("GameManager: restartButton is not assigned in the Inspector.");

        if (hubButton != null) hubButton.onClick.AddListener(GoToHub);
        else Debug.LogWarning("GameManager: hubButton is not assigned in the Inspector.");
    }

    void Update()
    {
        if (_isGameActive)
        {
            _timeElapsed += Time.deltaTime;
            DisplayTime(_timeElapsed);
        }
    }

    // Returns how many full 30-second intervals have elapsed — used by SparkSpawner
    public int GetElapsedIntervals()
    {
        return Mathf.FloorToInt(_timeElapsed / 30f);
    }

    public void AddScore(int points)
    {
        if (!_isGameActive) return;

        if (scoreText != null && !scoreText.gameObject.activeSelf)
            scoreText.gameObject.SetActive(true);

        _score += points;

        if (_score < 0) _score = 0;

        scoreText.text = _score.ToString();
        Debug.Log("Score Updated: " + _score + " (Added: " + points + ")");

        if (ScoreAnimator.Instance != null)
        {
            if (points > 0)
            {
                Debug.Log("GameManager: Calling PlayGain()");
                ScoreAnimator.Instance.PlayGain();
            }
            else if (points < 0)
            {
                Debug.Log("GameManager: Calling PlayLoss()");
                ScoreAnimator.Instance.PlayLoss();
            }
        }
        else
        {
            Debug.LogWarning("GameManager: ScoreAnimator.Instance is null!");
        }
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
        gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHub()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("HubWorld");
    }
}

