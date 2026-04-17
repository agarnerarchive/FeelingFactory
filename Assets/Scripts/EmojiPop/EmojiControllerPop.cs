using UnityEngine;

public class EmojiControllerPop : MonoBehaviour
{
    public static EmojiControllerPop Instance { get; private set; }

    [Header("References")]
    public SpriteRenderer emojiRenderer;
    public Animator animator;

    // Animator parameter names
    private static readonly int CelebrateTrigger = Animator.StringToHash("Celebrate");
    private static readonly int HurtTrigger      = Animator.StringToHash("Hurt");
    private static readonly int IdleTrigger       = Animator.StringToHash("Idle");

    private Vector3 originalScale;

private void Awake()
{
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
    originalScale = transform.localScale; // captures your 0.2 scale from the Inspector
}

    public void PlayCelebration()
{
    if (animator != null)
        animator.SetTrigger(CelebrateTrigger);

    StopAllCoroutines();
    transform.localScale = originalScale;
    StartCoroutine(BounceRoutine(0.3f, 1.35f));
}

public void PlayHurt()
{
    if (animator != null)
        animator.SetTrigger(HurtTrigger);

    StopAllCoroutines();
    transform.localScale = originalScale;
    StartCoroutine(ShakeRoutine());
}

public void SetEmoji(Sprite sprite)
{
    if (emojiRenderer != null)
        emojiRenderer.sprite = sprite;

    if (animator != null)
        animator.SetTrigger(IdleTrigger);

    StopAllCoroutines();
    transform.localScale = originalScale;
    StartCoroutine(ScalePop());
}

    private System.Collections.IEnumerator ScalePop()
{
    float t = 0f;
    float duration = 0.3f;

    while (t < duration)
    {
        t += Time.deltaTime;
        float progress = t / duration;
        float s;
        if (progress < 0.5f)
            s = Mathf.Lerp(0f, 1.2f, progress * 2f);
        else
            s = Mathf.Lerp(1.2f, 1f, (progress - 0.5f) * 2f);

        transform.localScale = originalScale * s;
        yield return null;
    }

    transform.localScale = originalScale;
}

    private System.Collections.IEnumerator BounceRoutine(float duration, float peak)
{
    float t = 0f;
    while (t < duration)
    {
        t += Time.deltaTime;
        float s = 1f + Mathf.Sin(t / duration * Mathf.PI) * (peak - 1f);
        transform.localScale = originalScale * s;
        yield return null;
    }
    transform.localScale = originalScale;
}

    private System.Collections.IEnumerator ShakeRoutine()
{
    Vector3 origin = transform.position;
    float t = 0f;
    float duration = 0.4f;
    float magnitude = 0.08f;
    while (t < duration)
    {
        t += Time.deltaTime;
        float x = Random.Range(-1f, 1f) * magnitude * (1f - t / duration);
        transform.position = origin + new Vector3(x, 0, 0);
        yield return null;
    }
    transform.position = origin;
    transform.localScale = originalScale;
}
}
