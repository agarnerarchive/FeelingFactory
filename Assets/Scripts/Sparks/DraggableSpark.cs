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


    [Header("Settings")]
    public string emotionType; 
    public float bounceForce = 3f;

    [Header("Feedback Settings")]
    private float _feedbackCooldown = 1.0f; 
    private float _lastFeedbackTime = 0f;

    void Awake()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        
        // Initial setup for the trail
        if (_trailRenderer != null) 
        {
            _trailRenderer.emitting = false;
        }

        // Initial "Big Feeling" push
        _rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * bounceForce;
    }

    void Update()
    {
        if (_isExpiring) return; // Stop input if we are currently fading out
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

    // Inside OnTriggerStay2D
private void OnTriggerStay2D(Collider2D collision)
{
    if (_isDragging && collision.gameObject.name.Contains("Jar"))
    {
        CameraShake shaker = _mainCamera.GetComponent<CameraShake>();

        animator.PlayOneShot.Spark;

        // 1. Correct Jar
        if (collision.gameObject.name.Contains(emotionType))
        {
            if (!_isExpiring) StartCoroutine(FadeOutAndDestroy());
        }
        // 2. Wrong Jar
        else
        {
            if (Time.time > _lastFeedbackTime + _feedbackCooldown)
            {
                // Trigger a longer, more violent rumble
                if (shaker != null) shaker.Shake(0.3f, 0.2f); 

                ShowFeedback("Not that one!", Color.red);
                _lastFeedbackTime = Time.time;
                _rb.AddForce((transform.position - collision.transform.position).normalized * 8f, ForceMode2D.Impulse);
            }
        }
    }
}

// Inside FadeOutAndDestroy
private IEnumerator FadeOutAndDestroy()
{
    _isExpiring = true;
    _isDragging = false;

    // Trigger a quick, subtle "success" pop
    CameraShake shaker = _mainCamera.GetComponent<CameraShake>();
    if (shaker != null) shaker.Shake(0.1f, 0.06f); 

    GameManager.Instance.AddScore(1);

    if (_spriteRenderer != null) _spriteRenderer.enabled = false;
    _rb.simulated = false; 

    if (_trailRenderer != null) _trailRenderer.emitting = false;
    float trailLife = _trailRenderer != null ? _trailRenderer.time : 0.1f;
    yield return new WaitForSeconds(trailLife);

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



