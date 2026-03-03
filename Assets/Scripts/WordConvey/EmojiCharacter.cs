// Assets/Scripts/EmojiCharacter.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EmojiCharacter : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] negativeEmojis;
    public Sprite positiveEmoji;

    [Header("References")]
    public SpriteRenderer meterFill;   // the green fill bar
    public Transform      coverObject; // the cover square Transform
    public Animator       emojiAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip goodPhraseClip, badPhraseClip, meterFullClip, coverClip;

    [Header("Meter Settings")]
    public float meterFillPerGoodPhrase = 0.25f;  // 4 good = full
    public float meterMaxHeight         = 3f;     // world units, matches MeterFill scale.y at 100%

    [Header("Timing")]
    public float positiveDuration   = 2f;
    public float coverAnimDuration  = 0.4f;
    public float shakeDuration      = 0.5f;
    public float shakeStrength      = 0.3f;      // world units

    // Meter fill base position — set this to your MeterFill's bottom-aligned Y
    [Header("Meter Anchor")]
    public float meterBottomY = -3.5f;  // world Y of the bottom of the meter bar

    private SpriteRenderer sr;
    private float  currentMeter    = 0f;
    private bool   isPositive      = false;
    private bool   isTransitioning = false;
    private int    emojiIndex      = 0;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void Start()
    {
        sr.sprite = negativeEmojis[0];
        SetMeterFill(0f, instant: true);
        coverObject.gameObject.SetActive(false);
    }

    public void ReceivePhrase(PhraseCard card)
    {
        if (isTransitioning || isPositive) return;

        if (card.isGood)
        {
            PlayClip(goodPhraseClip);
            currentMeter = Mathf.Clamp01(currentMeter + meterFillPerGoodPhrase);
            StartCoroutine(AnimateMeter(currentMeter));
            emojiAnimator?.SetTrigger("Happy");
            if (currentMeter >= 1f) StartCoroutine(MeterFullSequence());
        }
        else
        {
            PlayClip(badPhraseClip);
            emojiAnimator?.SetTrigger("Sad");
            StartCoroutine(ShakeThis(transform, 0.3f, 0.1f));
        }

        Destroy(card.gameObject);
    }

    // ─── Meter Fill ────────────────────────────────────────────────────────
    // We scale the fill quad on Y and offset its position so it grows upward
    void SetMeterFill(float t, bool instant = false)
    {
        float targetHeight = meterMaxHeight * t;
        Vector3 targetScale = new Vector3(meterFill.transform.localScale.x, targetHeight, 1f);
        Vector3 targetPos   = new Vector3(
            meterFill.transform.position.x,
            meterBottomY + targetHeight * 0.5f,
            meterFill.transform.position.z);

        if (instant)
        {
            meterFill.transform.localScale = targetScale;
            meterFill.transform.position   = targetPos;
        }
        // Non-instant is handled in AnimateMeter coroutine
    }

    IEnumerator AnimateMeter(float target)
    {
        float startT  = currentMeter - meterFillPerGoodPhrase;
        float elapsed = 0f, duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Lerp(startT, target, elapsed / duration);
            float h = meterMaxHeight * t;
            meterFill.transform.localScale = new Vector3(
                meterFill.transform.localScale.x, h, 1f);
            meterFill.transform.position = new Vector3(
                meterFill.transform.position.x,
                meterBottomY + h * 0.5f,
                meterFill.transform.position.z);
            yield return null;
        }
        SetMeterFill(target, instant: true);
    }

    // ─── Round Sequence ────────────────────────────────────────────────────
    IEnumerator MeterFullSequence()
    {
        isTransitioning = true;
        FindFirstObjectByType<ConveyorBelt>()?.SetRunning(false);
        PlayClip(meterFullClip);

        // 1. Go positive
        isPositive = true;
        sr.sprite = positiveEmoji;
        emojiAnimator?.SetTrigger("Celebrate");
        yield return new WaitForSeconds(positiveDuration);

        // 2. Cover slams down
        PlayClip(coverClip);
        yield return SlideCover(down: true);

        // 3. Screen shake
        yield return ShakeThis(Camera.main.transform, shakeDuration, shakeStrength);

        // 4. Reset behind cover
        emojiIndex++;
        currentMeter = 0f;
        SetMeterFill(0f, instant: true);
        isPositive = false;
        sr.sprite = negativeEmojis[emojiIndex % negativeEmojis.Length];
        yield return new WaitForSeconds(0.3f);

        // 5. Cover reveals new emoji
        yield return SlideCover(down: false);

        isTransitioning = false;
        FindFirstObjectByType<ConveyorBelt>()?.SetRunning(true);
        GameManagerConvey.Instance?.NextRound();
    }

    IEnumerator SlideCover(bool down)
    {
        coverObject.gameObject.SetActive(true);

        // Cover sits above the emoji when hidden, and on top of the emoji when shown
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