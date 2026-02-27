// ProgressBar.cs
// Attach to your ProgressBar UI Panel.
 
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class ProgressBar : MonoBehaviour
{
    public static ProgressBar Instance { get; private set; }
 
    [Header("UI References")]
    public Slider slider;
    public TextMeshProUGUI scoreLabel;    // Shows "3 / 5"
    public Image fillImage;               // Optional: change colour near completion
 
    [Header("Colours")]
    public Color normalColor   = new Color(0.2f, 0.7f, 0.3f);
    public Color completeColor = new Color(1.0f, 0.8f, 0.0f);
 
    private int currentScore;
    private int targetScore;
    private bool goalReached = false;
 
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
 
    // ── Public API ────────────────────────────────────────────────────────────
    public void Initialise(int target)
    {
        targetScore  = Mathf.Max(1, target);
        currentScore = 0;
        goalReached  = false;
 
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = targetScore;
            slider.value    = 0;
        }
 
        if (fillImage != null)
            fillImage.color = normalColor;
 
        RefreshLabel();
    }
 
    public void AddScore(int delta)
    {
        if (goalReached) return;   // Prevent double-triggering
 
        currentScore = Mathf.Max(0, currentScore + delta);
 
        if (slider != null) slider.value = currentScore;
        RefreshLabel();
 
        // Check completion
        if (currentScore >= targetScore)
        {
            goalReached = true;
            if (fillImage != null) fillImage.color = completeColor;
            GameManagerConvey.Instance.OnProgressComplete();
        }
    }
 
    // ── Private ───────────────────────────────────────────────────────────────
    private void RefreshLabel()
    {
        if (scoreLabel != null)
            scoreLabel.text = $"{currentScore} / {targetScore}";
    }
}


