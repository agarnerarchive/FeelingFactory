using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManagerPop : MonoBehaviour
{
    public static GameManagerPop Instance { get; private set; }

    [Header("Emoji Sequence")]
    public List<Sprite> emojiSequence;      // drag in order: 😐 😊 😄 😁 etc.
    public int currentEmojiIndex = 0;

    [Header("Game State")]
    public bool isGameActive = true;
    public int score = 0;

    public delegate void OnEmojiChanged(Sprite newEmoji);
    public event OnEmojiChanged EmojiChangedEvent;

    public delegate void OnScoreChanged(int newScore);
    public event OnScoreChanged ScoreChangedEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (emojiSequence.Count == 0)
            Debug.LogWarning("GameManagerPop: No emojis assigned in emojiSequence!");
    }

    public void AddScore(int amount)
    {
        score += amount;
        ScoreChangedEvent?.Invoke(score);
    }

    // Called by HappinessMeter when meter is full
    public void AdvanceEmoji()
    {
        StartCoroutine(AdvanceEmojiRoutine());
    }

    private IEnumerator AdvanceEmojiRoutine()
{
    isGameActive = false;

    if (EmojiControllerPop.Instance != null)
        EmojiControllerPop.Instance.PlayCelebration();
    
    yield return new WaitForSeconds(1.5f);

    currentEmojiIndex++;
    if (currentEmojiIndex >= emojiSequence.Count)
        currentEmojiIndex = 0;

    Sprite nextEmoji = emojiSequence[currentEmojiIndex];

    if (EmojiControllerPop.Instance != null)
        EmojiControllerPop.Instance.SetEmoji(nextEmoji);

    if (HappinessMeter.Instance != null)
        HappinessMeter.Instance.ResetMeter();

    EmojiChangedEvent?.Invoke(nextEmoji);

    yield return new WaitForSeconds(0.5f);

    isGameActive = true; // this line must always be reached
}
}