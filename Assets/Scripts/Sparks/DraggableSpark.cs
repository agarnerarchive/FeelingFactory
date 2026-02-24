using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class DraggableSpark : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Camera _mainCamera;
    private SpriteRenderer _spriteRenderer;
    private TrailRenderer _trailRenderer;
    //private Animator _animator; // Added Animator reference
    private bool _isDragging = false;
    private Vector3 _worldPos;
    private bool _isExpiring = false;
    AudioSource splash;
    public GameObject sparksplash;

    [Header("Settings")]
    public string emotionType; 
    public float bounceForce = 3f;
    public string destroyAnimationTrigger = "OnDestroy"; // Name of your trigger in the Animator

    [Header("Feedback Settings")]
    private float _feedbackCooldown = 1.0f; 
    private float _lastFeedbackTime = 0f;
    
    [Header("Juice Settings")]
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.5f;
    public float wobbleAmount = 15f; // Max rotation degrees
    public float wobbleSpeed = 20f;  // Speed of the oscillation
    private Vector3 _originalScale;
    private Coroutine _juiceCoroutine;

    void Awake()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        //_animator = GetComponent<Animator>(); // Cache the animator
        splash = GetComponent<AudioSource>();
        _originalScale = transform.localScale;
        
        if (_trailRenderer != null) _trailRenderer.emitting = false;

        _rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * bounceForce;
    }

    void Update()
    {
        if (_isExpiring) return;
        HandleInput();
    }

    void HandleInput()
    {
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        Vector2 screenPos = Vector2.zero;
        bool isPressing = false;
        bool pressStarted = false;

        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            screenPos = touch.primaryTouch.position.ReadValue();
            isPressing = true;
            pressStarted = touch.primaryTouch.press.wasPressedThisFrame;
        }
        else if (mouse != null)
        {
            screenPos = mouse.position.ReadValue();
            isPressing = mouse.leftButton.isPressed;
            pressStarted = mouse.leftButton.wasPressedThisFrame;
        }

        _worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        _worldPos.z = 0;

        if (pressStarted)
        {
            Collider2D hit = Physics2D.OverlapPoint(_worldPos);
            if (hit != null && hit.transform == transform) 
            {
                _isDragging = true;
                if (_trailRenderer != null) _trailRenderer.emitting = true;
                _rb.linearVelocity = Vector2.zero; 
                _rb.gravityScale = 0;
            }
        }

        if (isPressing && _isDragging)
        {
            _rb.MovePosition(_worldPos);
        }
        else if (!isPressing && _isDragging)
        {
            _isDragging = false;
            if (_trailRenderer != null) _trailRenderer.emitting = false;
            _rb.linearVelocity = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) * bounceForce;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_isDragging && collision.gameObject.name.Contains("Jar"))
        {
            CameraShake shaker = _mainCamera.GetComponent<CameraShake>();

            if (collision.gameObject.name.Contains(emotionType))
            {
                if (!_isExpiring) StartCoroutine(FadeOutAndDestroy());
            }
            else
            {
                if (Time.time > _lastFeedbackTime + _feedbackCooldown)
                {
                    if (shaker != null) shaker.Shake(0.3f, 0.2f); 
                    ShowFeedback("Not that one!", Color.red);
                    _lastFeedbackTime = Time.time;
                    _rb.AddForce((transform.position - collision.transform.position).normalized * 8f, ForceMode2D.Impulse);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
{
    if (_isExpiring) return;

    if (_juiceCoroutine != null) StopCoroutine(_juiceCoroutine);
    _juiceCoroutine = StartCoroutine(PulseAndWobble());
}

private IEnumerator PulseAndWobble()
{
    float elapsed = 0f;
    
    // 1. Initial burst: Scale up immediately
    transform.localScale = _originalScale * scaleMultiplier;

    while (elapsed < scaleDuration)
    {
        elapsed += Time.deltaTime;
        float percent = elapsed / scaleDuration;

        // 2. Smoothly Scale back to normal
        transform.localScale = Vector3.Lerp(_originalScale * scaleMultiplier, _originalScale, percent);

        // 3. Apply Wobble: Use Sine wave for a "wiggle" that fades out
        // The (1 - percent) makes the wiggle smaller as the effect ends
        float zRotation = Mathf.Sin(elapsed * wobbleSpeed) * wobbleAmount * (1 - percent);
        transform.rotation = Quaternion.Euler(0, 0, zRotation);

        yield return null;
    }

    // Reset to perfect defaults
    transform.localScale = _originalScale;
    transform.rotation = Quaternion.identity;
}

private IEnumerator PulseScale()
{
    // 1. Instant Scale Up
    transform.localScale = _originalScale * scaleMultiplier;

    // 2. Smoothly Scale Back Down
    float elapsed = 0f;
    while (elapsed < scaleDuration)
    {
        elapsed += Time.deltaTime;
        float percent = elapsed / scaleDuration;
        
        // Lerp from the current (large) scale back to original
        transform.localScale = Vector3.Lerp(_originalScale * scaleMultiplier, _originalScale, percent);
        
        yield return null; // Wait for next frame
    }

    // Ensure it's exactly back to original
    transform.localScale = _originalScale;
}


    private IEnumerator FadeOutAndDestroy()
{
    _isExpiring = true;
    _isDragging = false;
    GetComponent<Collider2D>().enabled = false; // Disable physics immediately

    // 1. Spawn the Particle Effect at current position
    if (sparksplash != null)
    {
        // Instantiate the prefab
        GameObject effect = Instantiate(sparksplash, transform.position, Quaternion.identity);
        
        // Ensure it cleans itself up after 2 seconds (or use effect.GetComponent<ParticleSystem>().main.duration)
        Destroy(effect, 1f); 
    }

    // 2. Play Sound and Shake
    if (splash != null) splash.Play();
    CameraShake shaker = _mainCamera.GetComponent<CameraShake>();
    if (shaker != null) shaker.Shake(0.1f, 0.06f);

    GameManager.Instance.AddScore(1);

    // 3. Hide the spark immediately while the sound/trail finishes
    if (_spriteRenderer != null) _spriteRenderer.enabled = false;
    _rb.simulated = false; 

    // 4. Wait for trail to fade before final destruction
    if (_trailRenderer != null) 
    {
        _trailRenderer.emitting = false;
        yield return new WaitForSeconds(_trailRenderer.time);
    }

    Destroy(gameObject);
}

    void ShowFeedback(string message, Color color)
    {
        GameObject textObj = new GameObject("FeedbackText");
        textObj.transform.position = transform.position + Vector3.up;
        var tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 4;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        Destroy(textObj, 0.5f);
    }
}




