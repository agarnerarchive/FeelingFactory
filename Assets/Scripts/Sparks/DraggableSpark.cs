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
    private bool _isDragging = false;
    private Vector3 _worldPos;
    private bool _isExpiring = false;
    private AudioSource _splash;

    [Header("References")]
    public GameObject sparksplash;

    [Header("Settings")]
    public string emotionType;
    public float bounceForce = 3f;

    [Header("Feedback Settings")]
    private float _feedbackCooldown = 1.0f;
    private float _lastFeedbackTime = 0f;

    [Header("Juice Settings")]
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.5f;
    public float wobbleAmount = 15f;
    public float wobbleSpeed = 20f;
    private Vector3 _originalScale;
    private Coroutine _juiceCoroutine;

    void Awake()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _splash = GetComponent<AudioSource>();
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
            _rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * bounceForce;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_isExpiring || !_isDragging) return;

        if (collision.gameObject.name.Contains("Jar"))
        {
            CameraShake shaker = _mainCamera.GetComponent<CameraShake>();

            if (collision.gameObject.name.Contains(emotionType))
            {
                // Correct jar — gain a point and flash green
                GameManager.Instance.AddScore(1);
                ScoreAnimator.Instance?.PlayGain();
                StartCoroutine(FadeOutAndDestroy());
            }
            else if (Time.time > _lastFeedbackTime + _feedbackCooldown)
            {
                if (shaker != null) shaker.Shake(0.3f, 0.2f);
                ShowFeedback("Not that one!", Color.red);
                _lastFeedbackTime = Time.time;
                _rb.AddForce((transform.position - collision.transform.position).normalized * 8f, ForceMode2D.Impulse);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isExpiring) return;

        DraggableSpark other = collision.gameObject.GetComponent<DraggableSpark>();

        if (other != null && !other._isExpiring && (_isDragging || other._isDragging))
        {
            // Lock both sparks immediately to prevent duplicate handling
            this._isExpiring = true;
            other._isExpiring = true;

            // Symmetry breaker: only the spark with the lower ID deducts the score,
            // so the penalty fires exactly once per collision
            if (this.gameObject.GetInstanceID() < other.gameObject.GetInstanceID())
            {
                GameManager.Instance.AddScore(-1);
                ScoreAnimator.Instance?.PlayLoss();
            }

            StartCoroutine(FadeOutAndDestroy());
            other.StartCoroutine(other.FadeOutAndDestroy());
        }
    }

    public IEnumerator FadeOutAndDestroy()
    {
        _isExpiring = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (sparksplash != null)
        {
            GameObject effect = Instantiate(sparksplash, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        if (_splash != null) _splash.Play();

        CameraShake shaker = _mainCamera.GetComponent<CameraShake>();
        if (shaker != null) shaker.Shake(0.1f, 0.06f);

        if (_spriteRenderer != null) _spriteRenderer.enabled = false;
        _rb.simulated = false;

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


    




