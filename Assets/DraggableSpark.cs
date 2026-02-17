using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DraggableSpark : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Camera _mainCamera;
    private bool _isDragging = false;
    private Vector3 _worldPos;

    [Header("Settings")]
    public string emotionType; 
    public float bounceForce = 3f;

    void Awake()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        
        // Initial "Big Feeling" push
        _rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * bounceForce;
    }

    void Update()
    {
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
                // IMPORTANT: We keep simulated = true so it can still "hit" the jars!
                _rb.linearVelocity = Vector2.zero; 
                _rb.gravityScale = 0;
            }
        }

        if (isPressing && _isDragging)
        {
            // Instead of just setting position, we move the Rigidbody
            _rb.MovePosition(_worldPos);
        }
        else if (!isPressing && _isDragging)
        {
            _isDragging = false;
            _rb.linearVelocity = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) * bounceForce;
        }
    }

    private float _feedbackCooldown = 1.0f; // Seconds to wait between messages
private float _lastFeedbackTime = 0f;

private void OnTriggerStay2D(Collider2D collision)
{
    if (_isDragging && collision.gameObject.name.Contains("Jar"))
    {
        // 1. Correct Jar
        if (collision.gameObject.name.Contains(emotionType))
        {
            GameManager.Instance.AddScore(1);
            Destroy(gameObject);
        }
        // 2. Wrong Jar (with Cooldown)
        else
        {
            if (Time.time > _lastFeedbackTime + _feedbackCooldown)
            {
                ShowFeedback("Not that one!", Color.red);
                _lastFeedbackTime = Time.time; // Reset the timer
                
                // Add a small "haptic" bounce away from the wrong jar
                _rb.AddForce((transform.position - collision.transform.position).normalized * 5f, ForceMode2D.Impulse);
            }
        }
    }
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


