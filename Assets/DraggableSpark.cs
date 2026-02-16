using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DraggableSpark : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Camera _mainCamera;
    private bool _isDragging = false;
    private Vector3 _worldPos; // Moved here so the whole script can see it!

    [Header("Settings")]
    public string emotionType; // Set this to "Happy" or "Sad" in Inspector
    public float bounceForce = 3f;

    void Awake()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        
        // Give it an initial "Big Feeling" bounce
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

        // 1. Get the screen position from either Touch or Mouse
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

        // 2. Convert that screen point to World Position
        _worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        _worldPos.z = 0;

        // 3. Logic for Picking up and Moving
        if (pressStarted)
        {
            Collider2D hit = Physics2D.OverlapPoint(_worldPos);
            if (hit != null && hit.transform == transform) 
            {
                _isDragging = true;
                _rb.simulated = false; // Turn off physics while holding
            }
        }

        if (isPressing && _isDragging)
        {
            transform.position = _worldPos;
        }
        else if (!isPressing && _isDragging)
        {
            _isDragging = false;
            _rb.simulated = true; // Drop it back into physics!
            // Give it a little toss based on movement
            _rb.linearVelocity = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) * bounceForce;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit a Jar
        if (collision.gameObject.name.Contains("Jar"))
        {
            if (collision.gameObject.name.Contains(emotionType))
            {
                GameManager.Instance.AddScore(1);
                Destroy(gameObject);
            }
            else
            {
                // WRONG JAR: Show the text we talked about!
                ShowFeedback("That doesn't feel right...", Color.red);
                _rb.linearVelocity = (transform.position - collision.transform.position).normalized * (bounceForce * 2);
            }
        }
    }

    void ShowFeedback(string message, Color color)
    {
        GameObject textObj = new GameObject("FeedbackText");
        textObj.transform.position = transform.position + Vector3.up;
        var tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 5;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        // Make it disappear after 1 second
        Destroy(textObj, 1f);
    }
}

