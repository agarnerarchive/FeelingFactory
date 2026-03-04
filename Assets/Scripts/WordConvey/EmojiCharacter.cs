// Assets/Scripts/EmojiCharacter.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EmojiCharacter : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] negativeEmojis;
    public Sprite positiveEmoji;

    [Header("References")]
    public Slider    meterSlider;
    public Transform coverObject;
    public Animator  emojiAnimator;   // ← assign in Inspector

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip goodPhraseClip, badPhraseClip, meterFullClip, coverClip;

    [Header("Meter Settings")]
    public int phrasesRequired = 4;

    [Header("Timing")]
    public float positiveDuration  = 2f;
    public float coverAnimDuration = 0.4f;
    public float shakeDuration     = 0.5f;
    public float shakeStrength     = 0.3f;

    private SpriteRenderer sr;
    private int  correctCount    = 0;
    private bool isPositive      = false;
    private bool isTransitioning = false;
    private int  emojiIndex      = 0;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void Start()
{
    // Guard against empty array
    if (negativeEmojis == null || negativeEmojis.Length == 0)
    {
        Debug.LogError("EmojiCharacter: No negative emojis assigned!");
        return;
    }

    isPositive      = false;   // explicitly set to be safe
    isTransitioning = false;
    correctCount    = 0;

    sr.sprite         = negativeEmojis[0];
    meterSlider.value = 0f;
    coverObject.gameObject.SetActive(false);

    Debug.Log("EmojiCharacter started correctly.");
}

public void ReceivePhrase(PhraseCard card)
{
    // Temporary debug — remove once working
    Debug.Log($"ReceivePhrase called. isPositive={isPositive} isTransitioning={isTransitioning}");

    if (isTransitioning || isPositive) return;

    if (card.isGood)
    {
        PlayClip(goodPhraseClip);
        emojiAnimator?.SetTrigger("Correct");
        correctCount = Mathf.Min(correctCount + 1, phrasesRequired);
        StartCoroutine(AnimateMeter((float)correctCount / phrasesRequired));
        if (correctCount >= phrasesRequired) StartCoroutine(MeterFullSequence());
    }
    else
    {
        PlayClip(badPhraseClip);
        emojiAnimator?.SetTrigger("Wrong");
    }

    Destroy(card.gameObject);
}

    // ─── Meter ─────────────────────────────────────────────────────────────

    IEnumerator AnimateMeter(float targetValue)
    {
        float startValue = meterSlider.value;
        float elapsed = 0f, duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            meterSlider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }

        meterSlider.value = targetValue;
    }

    // ─── Round Sequence ─────────────────────────────────────────────────────

    IEnumerator MeterFullSequence()
    {
        isTransitioning = true;
        FindFirstObjectByType<ConveyorBelt>()?.SetRunning(false);
        PlayClip(meterFullClip);

        isPositive = true;
        sr.sprite = positiveEmoji;
        yield return new WaitForSeconds(positiveDuration);

        PlayClip(coverClip);
        yield return SlideCover(down: true);

        yield return ShakeThis(Camera.main.transform, shakeDuration, shakeStrength);

        emojiIndex++;
        correctCount = 0;
        meterSlider.value = 0f;
        isPositive = false;
        sr.sprite = negativeEmojis[emojiIndex % negativeEmojis.Length];

        yield return new WaitForSeconds(0.3f);

        yield return SlideCover(down: false);

        isTransitioning = false;
        FindFirstObjectByType<ConveyorBelt>()?.SetRunning(true);
        GameManagerConvey.Instance?.NextRound();
    }

    // ─── Cover ──────────────────────────────────────────────────────────────

    IEnumerator SlideCover(bool down)
    {
        coverObject.gameObject.SetActive(true);

        float emojiHalfH = transform.localScale.y * 0.5f;
        float coverHalfH = coverObject.localScale.y * 0.5f;

        Vector3 shownPos  = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);
        Vector3 hiddenPos = new Vector3(shownPos.x, transform.position.y + emojiHalfH + coverHalfH + 0.1f, shownPos.z);

        Vector3 from = down ? hiddenPos : shownPos;
        Vector3 to   = down ? shownPos  : hiddenPos;

        float elapsed = 0f;
        while (elapsed < coverAnimDuration)
        {
            elapsed += Time.deltaTime;
            coverObject.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / coverAnimDuration));
            yield return null;
        }
        coverObject.position = to;
        if (!down) coverObject.gameObject.SetActive(false);
    }

    // ─── Utilities ──────────────────────────────────────────────────────────

    IEnumerator ShakeThis(Transform target, float duration, float strength)
    {
        Vector3 origin  = target.position;
        float   elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float s = strength * (1f - elapsed / duration);
            target.position = origin + (Vector3)(Random.insideUnitCircle * s);
            yield return null;
        }
        target.position = origin;
    }

    void PlayClip(AudioClip clip) { if (audioSource && clip) audioSource.PlayOneShot(clip); }
}