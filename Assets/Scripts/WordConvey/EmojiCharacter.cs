// Assets/Scripts/EmojiCharacter.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EmojiCharacter : MonoBehaviour
{
    [Header("Emoji Data (assign all 3 in order)")]
    public EmojiData[] emojiSequence;

    [Header("References")]
    public Slider    meterSlider;
    public Transform coverObject;
    public Animator  emojiAnimator;

    [Header("Base Animator Controller")]
    public RuntimeAnimatorController baseController;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip goodPhraseClip, badPhraseClip, meterFullClip, coverClip, wobble;

    [Header("Meter Settings")]
    public int phrasesRequired = 4;

    [Header("Timing")]
    public float positiveDuration  = 2f;
    public float coverAnimDuration = 0.4f;
    public float shakeDuration     = 0.5f;
    public float shakeStrength     = 0.3f;

    private SpriteRenderer            sr;
    private AnimatorOverrideController overrideController;
    private int  currentIndex    = 0;
    private int  correctCount    = 0;
    private bool isPositive      = false;
    private bool isTransitioning = false;

    EmojiData Current => emojiSequence[currentIndex % emojiSequence.Length];

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Set sprite immediately so it never flickers
        if (emojiSequence != null && emojiSequence.Length > 0)
            sr.sprite = emojiSequence[0].negativeSprite;
    }

    void Start()
{
    if (emojiSequence == null || emojiSequence.Length == 0)
    {
        Debug.LogError("EmojiCharacter: No EmojiData assigned!");
        return;
    }

    // Set up override controller
    overrideController = new AnimatorOverrideController(baseController);
    emojiAnimator.runtimeAnimatorController = overrideController;

    isPositive      = false;
    isTransitioning = false;
    correctCount    = 0;

    sr.sprite         = Current.negativeSprite;
    meterSlider.value = 0f;
    coverObject.gameObject.SetActive(true);

    FindFirstObjectByType<ConveyorBelt>()?.SetEmojiData(Current);

    // Apply clips last so animator is fully ready
    ApplyAnimationClips(Current);
}
    // ─── Clip Swapping ──────────────────────────────────────────────────────

    void ApplyAnimationClips(EmojiData data)
{
    if (emojiAnimator == null || overrideController == null || data == null) return;

    if (data.idleClip    != null) overrideController["Base_Idle"]    = data.idleClip;
    if (data.correctClip != null) overrideController["Base_Correct"] = data.correctClip;
    if (data.wrongClip   != null) overrideController["Base_Wrong"]   = data.wrongClip;
    if (data.idleClip    != null) overrideController["Base_SpawnIn"] = data.idleClip;
    if (data.fullClip    != null) overrideController["Base_Full"]    = data.fullClip;

    StartCoroutine(PlayIdleNextFrame());
}

// Wait one frame so the override controller finishes applying
// before telling the animator to play
IEnumerator PlayIdleNextFrame()
{
    yield return null;
    emojiAnimator.Rebind();
    emojiAnimator.Update(0f);
    emojiAnimator.Play("Idle", 0, 0f);
}
    // ─── Phrase Receiving ───────────────────────────────────────────────────

    public void ReceivePhrase(PhraseCard card)
    {
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

    // ─── Meter ──────────────────────────────────────────────────────────────

    IEnumerator AnimateMeter(float targetValue)
    {
        float startValue = meterSlider.value;
        float elapsed    = 0f, duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed           += Time.deltaTime;
            meterSlider.value  = Mathf.Lerp(startValue, targetValue, elapsed / duration);
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
        emojiAnimator?.SetTrigger("Full");
        yield return new WaitForSeconds(1.0f);

        // Show positive sprite
        isPositive = true;
        sr.sprite  = Current.positiveSprite;
        yield return new WaitForSeconds(positiveDuration);

        // Cover slams down
        PlayClip(coverClip);
        PlayClip(wobble);
        yield return SlideCover(down: true);

        // Screen shake
        yield return ShakeThis(Camera.main.transform, shakeDuration, shakeStrength);

        // Advance to next emoji
        currentIndex++;
        correctCount      = 0;
        meterSlider.value = 0f;
        isPositive        = false;
        sr.sprite         = Current.negativeSprite;

        // Swap in the new emoji's animation clips
        ApplyAnimationClips(Current);

        // Tell conveyor to use new emoji's phrases
        FindFirstObjectByType<ConveyorBelt>()?.SetEmojiData(Current);

        yield return new WaitForSeconds(0.3f);

        // Cover reveals new emoji
        yield return SlideCover(down: false);

        isTransitioning = false;
        FindFirstObjectByType<ConveyorBelt>()?.SetRunning(true);
        GameManagerConvey.Instance?.NextRound();
        PlayClip(coverClip);
    }

    // ─── Cover ──────────────────────────────────────────────────────────────

    IEnumerator SlideCover(bool down)
    {
        coverObject.gameObject.SetActive(true);

        float emojiHalfH = transform.localScale.y * 0.5f;
        float coverHalfH = coverObject.localScale.y * 0.5f;

        Vector3 shownPos  = new Vector3(transform.position.x, transform.position.y, transform.position.z - 5.0f);
        Vector3 hiddenPos = new Vector3(shownPos.x, transform.position.y + emojiHalfH + coverHalfH + 5.0f, shownPos.z);

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
        if (!down) coverObject.gameObject.SetActive(true);
    }

    // ─── Utilities ──────────────────────────────────────────────────────────

    IEnumerator ShakeThis(Transform target, float duration, float strength)
    {
        Vector3 origin  = target.position;
        float   elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float s  = strength * (1f - elapsed / duration);
            target.position = origin + (Vector3)(Random.insideUnitCircle * s);
            yield return null;
        }
        target.position = origin;
    }

    void PlayClip(AudioClip clip) { if (audioSource && clip) audioSource.PlayOneShot(clip); }
}