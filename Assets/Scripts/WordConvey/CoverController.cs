// CoverController.cs
// Attach to the Cover Panel (a UI Image that slides down from above).
 
using UnityEngine;
using System.Collections;
 
public class CoverController : MonoBehaviour
{
    [Header("Cover RectTransform")]
    public RectTransform coverPanel;
 
    [Header("Anchor Positions (Anchored Position Y)")]
    [Tooltip("Y position when cover is OFF screen above (e.g. 1400)")]
    public float hiddenY   = 1400f;
 
    [Tooltip("Y position when cover is FULLY down covering the emoji (e.g. 0)")]
    public float visibleY  = 0f;
 
    [Header("Timing")]
    public float LowerDuration = 0.8f;
    public float RaiseDuration = 0.8f;
 
    [Header("Animation Curves")]
    public AnimationCurve lowerCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve raiseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
 
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lowerSFX;
    public AudioClip raiseSFX;
 
    private Coroutine moveRoutine;
 
    private void Start()
    {
        // Ensure cover starts off-screen
        if (coverPanel != null)
        {
            Vector2 ap = coverPanel.anchoredPosition;
            ap.y = hiddenY;
            coverPanel.anchoredPosition = ap;
        }
    }
 
    // ── Public ────────────────────────────────────────────────────────────────
    public void LowerCover()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(SlideRoutine(hiddenY, visibleY,
                                                   LowerDuration, lowerCurve, lowerSFX));
    }
 
    public void RaiseCover()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(SlideRoutine(visibleY, hiddenY,
                                                   RaiseDuration, raiseCurve, raiseSFX));
    }
 
    // ── Private ───────────────────────────────────────────────────────────────
    private IEnumerator SlideRoutine(float fromY, float toY, float duration,
                                      AnimationCurve curve, AudioClip sfx)
    {
        if (sfx != null && audioSource != null)
            audioSource.PlayOneShot(sfx);
 
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = curve.Evaluate(elapsed / duration);
            Vector2 ap = coverPanel.anchoredPosition;
            ap.y = Mathf.Lerp(fromY, toY, t);
            coverPanel.anchoredPosition = ap;
            elapsed += Time.deltaTime;
            yield return null;
        }
 
        // Snap to final position
        Vector2 final = coverPanel.anchoredPosition;
        final.y = toY;
        coverPanel.anchoredPosition = final;
    }
}

