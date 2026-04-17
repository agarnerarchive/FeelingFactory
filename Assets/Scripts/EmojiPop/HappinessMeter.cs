using UnityEngine;
using UnityEngine.UI;

public class HappinessMeter : MonoBehaviour
{
    public static HappinessMeter Instance { get; private set; }

    [Header("UI")]
    public Slider meterSlider;
    public Image fillImage;

    [Header("Settings")]
    public float maxHappiness = 100f;
    public float currentHappiness = 0f;

    [Header("Colors")]
    public Color lowColor    = new Color(1f, 0.4f, 0.4f);
    public Color midColor    = new Color(1f, 0.85f, 0.2f);
    public Color highColor   = new Color(0.3f, 0.95f, 0.4f);

    private bool hasTriggeredAdvance = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (meterSlider != null)
        {
            meterSlider.minValue = 0f;
            meterSlider.maxValue = maxHappiness;
            meterSlider.value = 0f;
        }
    }

    public void AddHappiness(float amount)
    {
        currentHappiness = Mathf.Clamp(currentHappiness + amount, 0f, maxHappiness);
        UpdateUI();

        if (currentHappiness >= maxHappiness && !hasTriggeredAdvance)
        {
            hasTriggeredAdvance = true;
            GameManagerPop.Instance?.AdvanceEmoji();
        }
    }

    public void ResetMeter()
    {
        currentHappiness = 0f;
        hasTriggeredAdvance = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (meterSlider != null)
        {
            meterSlider.value = currentHappiness;
            // Animate toward target smoothly
        }

        if (fillImage != null)
        {
            float t = currentHappiness / maxHappiness;
            Color c = t < 0.5f
                ? Color.Lerp(lowColor, midColor, t * 2f)
                : Color.Lerp(midColor, highColor, (t - 0.5f) * 2f);
            fillImage.color = c;
        }
    }
}