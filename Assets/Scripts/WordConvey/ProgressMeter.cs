using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProgressMeter : MonoBehaviour
{
    [Header("References")]
    public Slider slider;
    public TextMeshProUGUI countLabel; // optional e.g. "2 / 3"

    [Header("Animation")]
    public float fillAnimDuration = 0.3f;

    private Coroutine animCoroutine;

    public void SetMax(int max)
    {
        slider.maxValue = max;
        slider.value = 0;
        UpdateLabel(0, max);
    }

    public void SetValue(int value)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateFill(slider.value, value));
        UpdateLabel(value, (int)slider.maxValue);
    }

    private IEnumerator AnimateFill(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fillAnimDuration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(from, to, elapsed / fillAnimDuration);
            yield return null;
        }
        slider.value = to;
    }

    private void UpdateLabel(int current, int max)
    {
        if (countLabel != null)
            countLabel.text = $"{current} / {max}";
    }
}