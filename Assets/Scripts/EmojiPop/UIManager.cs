using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score")]
    public TextMeshProUGUI scoreText;

    [Header("Feedback Pop Text")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 1.2f;

    [Header("Current Emoji Label")]
    public TextMeshProUGUI emojiNameText;

    private Coroutine feedbackRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (feedbackText != null) feedbackText.alpha = 0f;
        UpdateScore(0);

        // Subscribe to score changes
        if (GameManagerPop.Instance != null)
            GameManagerPop.Instance.ScoreChangedEvent += UpdateScore;
    }

    private void OnDestroy()
    {
        if (GameManagerPop.Instance != null)
            GameManagerPop.Instance.ScoreChangedEvent -= UpdateScore;
    }

    private void UpdateScore(int newScore)
    {
        if (scoreText != null) scoreText.text = $"Score: {newScore}";
    }

    public void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackRoutine(message, color));
    }

    private IEnumerator FeedbackRoutine(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.alpha = 1f;

        // Float upward
        Vector3 start = feedbackText.rectTransform.anchoredPosition;
        Vector3 end = start + new Vector3(0, 60f, 0);
        float t = 0f;

        while (t < feedbackDuration)
        {
            t += Time.deltaTime;
            float progress = t / feedbackDuration;
            feedbackText.rectTransform.anchoredPosition = Vector3.Lerp(start, end, progress);
            feedbackText.alpha = 1f - progress;
            yield return null;
        }

        feedbackText.alpha = 0f;
        feedbackText.rectTransform.anchoredPosition = start;
    }
}