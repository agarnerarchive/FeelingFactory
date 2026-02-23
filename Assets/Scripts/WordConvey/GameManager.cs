using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ConveyorGameManager : MonoBehaviour
{
    public static ConveyorGameManager Instance { get; private set; }

    [Header("Level Configuration")]
    public LevelData[] levels;

    [Header("Scene References")]
    public Image emojiImage;
    public ConveyorBelt conveyorBelt;
    public ProgressMeter progressMeter;

    [Header("Transition")]
    public float levelTransitionDelay = 1.2f;
    public GameObject levelCompleteEffect; // optional particle/animation object

    // Internal state
    private int currentLevelIndex = 0;
    private int correctDropsThisLevel = 0;
    private bool isTransitioning = false;

    public LevelData CurrentLevel => levels[currentLevelIndex];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        LoadLevel(0);
    }

    void LoadLevel(int index)
    {
        if (index >= levels.Length)
        {
            Debug.Log("All levels complete!");
            // Handle game complete — loop, show screen, etc.
            index = 0;
        }

        currentLevelIndex = index;
        correctDropsThisLevel = 0;
        isTransitioning = false;

        LevelData level = levels[index];

        // Set emoji
        emojiImage.sprite = level.emojiSprite;

        // Reset and start progress meter
        progressMeter.SetMax(level.correctDropsRequired);
        progressMeter.SetValue(0);

        // Start conveyor with this level's phrases
        conveyorBelt.StartBelt(level.allPhrases, level.beltSpeed);
    }

    /// <summary>
    /// Called by EmojiTarget when a phrase is dropped onto the emoji.
    /// Returns true if correct.
    /// </summary>
    public bool EvaluateDrop(string phrase)
    {
        if (isTransitioning) return false;

        bool correct = phrase.Trim().Equals(
            CurrentLevel.correctPhrase.Trim(),
            System.StringComparison.OrdinalIgnoreCase);

        if (correct)
        {
            correctDropsThisLevel++;
            progressMeter.SetValue(correctDropsThisLevel);

            if (correctDropsThisLevel >= CurrentLevel.correctDropsRequired)
            {
                StartCoroutine(TransitionToNextLevel());
            }
        }

        return correct;
    }

    private IEnumerator TransitionToNextLevel()
    {
        isTransitioning = true;
        conveyorBelt.StopBelt();

        if (levelCompleteEffect != null)
            levelCompleteEffect.SetActive(true);

        yield return new WaitForSeconds(levelTransitionDelay);

        if (levelCompleteEffect != null)
            levelCompleteEffect.SetActive(false);

        LoadLevel(currentLevelIndex + 1);
    }
}