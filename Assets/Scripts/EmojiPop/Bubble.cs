using UnityEngine;
using TMPro;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Bubble : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer bubbleRenderer;
    public SpriteRenderer contentSpriteRenderer;
    public TextMeshPro contentText;
    public ParticleSystem popParticles;

    [HideInInspector] public BubbleType bubbleType;
    public BubbleData bubbleData;

    private Vector2 driftDirection;
    private float speed;
    private bool isPopped = false;
    private Camera mainCam;
    private const float DestroyMargin = 2f;

    public void Initialize(BubbleType type, BubbleData data, Vector2 targetPos)
    {
        bubbleType = type;
        bubbleData = data;
        mainCam = Camera.main;

        bool isPositive = (type == BubbleType.PositivePhrase || type == BubbleType.PositiveEmoji);

        if (bubbleRenderer != null)
            bubbleRenderer.color = isPositive ? data.positiveColor : data.negativeColor;

        contentSpriteRenderer.gameObject.SetActive(false);
        contentText.gameObject.SetActive(false);

        switch (type)
        {
            case BubbleType.PositivePhrase:
                contentText.text = data.GetRandomPositivePhrase();
                contentText.color = new Color(0.1f, 0.5f, 0.1f);
                contentText.gameObject.SetActive(true);
                break;

            case BubbleType.NegativePhrase:
                contentText.text = data.GetRandomNegativePhrase();
                contentText.color = new Color(0.6f, 0.05f, 0.05f);
                contentText.gameObject.SetActive(true);
                break;

            case BubbleType.PositiveEmoji:
                contentSpriteRenderer.sprite = data.GetRandomPositiveEmojiSprite();
                contentSpriteRenderer.gameObject.SetActive(true);
                break;

            case BubbleType.NegativeEmoji:
                contentSpriteRenderer.sprite = data.GetRandomNegativeEmojiSprite();
                contentSpriteRenderer.gameObject.SetActive(true);
                break;
        }

        // FIX: assign to driftDirection so Update actually moves the bubble
        Vector2 toTarget = (targetPos - (Vector2)transform.position).normalized;
        driftDirection = toTarget;
        speed = Random.Range(0.6f, 1.4f);

        Debug.Log($"Bubble initialized | type: {type} | renderer null: {bubbleRenderer == null} | position: {transform.position}");

        StartCoroutine(WobbleRoutine());
    }

    private void Update()
    {
        if (isPopped) return;
        transform.Translate(driftDirection * speed * Time.deltaTime);
        if (IsOffScreen()) Destroy(gameObject);
    }

    private bool IsOffScreen()
    {
        if (mainCam == null) return false;
        float camH = mainCam.orthographicSize + DestroyMargin;
        float camW = mainCam.orthographicSize * mainCam.aspect + DestroyMargin;
        Vector2 p = transform.position;
        return p.x < -camW || p.x > camW || p.y < -camH || p.y > camH;
    }

    public void OnPopped()
{
    if (isPopped) return;
    isPopped = true;

    bool isNegative = bubbleType == BubbleType.NegativePhrase || bubbleType == BubbleType.NegativeEmoji;

    if (isNegative)
    {
        GameManagerPop.Instance?.AddScore(bubbleData.negativePoppedScore);
        AudioManager.Instance?.PlayPopNegative();
        UIManager.Instance?.ShowFeedback("Good block! ✓", Color.green);
    }
    else
    {
        GameManagerPop.Instance?.AddScore(-2);
        AudioManager.Instance?.PlayPopNegative();
        UIManager.Instance?.ShowFeedback("Oops! Let positives through!", Color.yellow);
    }

    PlayPopEffect(); // destruction handled inside PopAnimation
}

private void OnTriggerEnter2D(Collider2D other)
{
    if (isPopped) return;
    if (!other.CompareTag("MainEmoji")) return;
    isPopped = true;

    bool isPositive = bubbleType == BubbleType.PositivePhrase || bubbleType == BubbleType.PositiveEmoji;

    if (isPositive)
    {
        HappinessMeter.Instance?.AddHappiness(20f);
        GameManagerPop.Instance?.AddScore(bubbleData.positiveHitScore);
        AudioManager.Instance?.PlayPopPositive();
        UIManager.Instance?.ShowFeedback("💛 +Love", new Color(1f, 0.8f, 0f));
    }
    else
    {
        HappinessMeter.Instance?.AddHappiness(-10f);
        AudioManager.Instance?.PlayPopNegative();
        UIManager.Instance?.ShowFeedback("Ouch!", Color.red);
        EmojiControllerPop.Instance?.PlayHurt();
    }

    PlayPopEffect(); // destruction handled inside PopAnimation
}

    private void PlayPopEffect()
{
    if (popParticles != null)
    {
        popParticles.transform.parent = null;
        popParticles.Play();
        Destroy(popParticles.gameObject, 2f);
    }

    // Detach and destroy content children after the animation finishes
    if (contentSpriteRenderer != null)
    {
        contentSpriteRenderer.transform.parent = null;
        Destroy(contentSpriteRenderer.gameObject, 0.25f);
    }

    if (contentText != null)
    {
        contentText.transform.parent = null;
        Destroy(contentText.gameObject, 0.25f);
    }

    StartCoroutine(PopAnimation());
}

private System.Collections.IEnumerator PopAnimation()
{
    float duration = 0.25f;
    float t = 0f;
    Vector3 startScale = transform.localScale;
    Vector3 endScale   = startScale * 2.2f;

    Color startColor = bubbleRenderer != null ? bubbleRenderer.color : Color.white;
    Color endColor   = new Color(startColor.r, startColor.g, startColor.b, 0f);

    while (t < duration)
    {
        t += Time.deltaTime;
        float progress = t / duration;

        // Ease out — fast start, slows at end
        float eased = 1f - Mathf.Pow(1f - progress, 3f);

        transform.localScale = Vector3.Lerp(startScale, endScale, eased);

        if (bubbleRenderer != null)
            bubbleRenderer.color = Color.Lerp(startColor, endColor, eased);

        yield return null;
    }

    Destroy(gameObject);
}
    private System.Collections.IEnumerator WobbleRoutine()
    {
        float t = 0f;
        float wobbleSpeed = Random.Range(1.5f, 2.5f);
        float wobbleAmount = Random.Range(0.03f, 0.06f);
        Vector3 baseScale = transform.localScale;

        while (!isPopped)
        {
            t += Time.deltaTime * wobbleSpeed;
            transform.localScale = baseScale * (1f + Mathf.Sin(t) * wobbleAmount);
            yield return null;
        }
    }
}