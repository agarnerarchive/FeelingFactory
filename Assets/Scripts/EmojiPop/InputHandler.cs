using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

/// <summary>
/// Unified touch + mouse input handler.
/// Works on iPad (touch) and Web (mouse) via the Unity Input System.
/// </summary>
public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    private Camera mainCam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        EnhancedTouchSupport.Enable(); // required for touch via new Input System
    }

    private void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (!GameManagerPop.Instance.isGameActive) return;

        HandleTouchInput();
        HandleMouseInput();
    }

    private void HandleTouchInput()
    {
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        foreach (var touch in touches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                TryPopBubbleAtScreenPos(touch.screenPosition);
            }
        }
    }

    private void HandleMouseInput()
    {
        // Only use mouse if no touches (avoids double-firing on touch devices)
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPopBubbleAtScreenPos(Mouse.current.position.ReadValue());
        }
    }

    private void TryPopBubbleAtScreenPos(Vector2 screenPos)
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;

        // Small overlap circle to find bubbles near the tap
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.4f);
        foreach (var hit in hits)
        {
            Bubble bubble = hit.GetComponent<Bubble>();
            if (bubble != null)
            {
                bubble.OnPopped();
                return; // pop one bubble per tap
            }
        }
    }
}