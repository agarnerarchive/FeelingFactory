// Assets/Scripts/InputManager.cs
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Camera cam;
    private PhraseCard heldCard;

    void Start() => cam = Camera.main;

    void Update()
    {
        // Works for both mouse (WebGL) and touch (iPad)
        if (Input.touchCount > 0)
            HandleTouch(Input.GetTouch(0));
        else
            HandleMouse();
    }

    void HandleMouse()
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        if (Input.GetMouseButtonDown(0)) TryPickUp(worldPos);
        else if (Input.GetMouseButton(0) && heldCard != null) heldCard.Drag(worldPos);
        else if (Input.GetMouseButtonUp(0) && heldCard != null) Release(worldPos);
    }

    void HandleTouch(Touch touch)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(touch.position);
        worldPos.z = 0f;

        switch (touch.phase)
        {
            case TouchPhase.Began:    TryPickUp(worldPos); break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (heldCard != null) heldCard.Drag(worldPos);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (heldCard != null) Release(worldPos);
                break;
        }
    }

    void TryPickUp(Vector3 worldPos)
    {
        // Raycast to find a PhraseCard at this position
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null)
        {
            PhraseCard card = hit.GetComponent<PhraseCard>();
            if (card != null)
            {
                heldCard = card;
                heldCard.BeginDrag(worldPos);
            }
        }
    }

    void Release(Vector3 worldPos)
    {
        heldCard.EndDrag();

        // Check if dropped onto emoji
        EmojiCharacter emoji = FindFirstObjectByType<EmojiCharacter>();
        if (emoji != null)
        {
            Collider2D emojiCol = emoji.GetComponent<Collider2D>();
            if (emojiCol != null && emojiCol.OverlapPoint(worldPos))
            {
                emoji.ReceivePhrase(heldCard);
                heldCard = null;
                return;
            }
        }

        // Not dropped on emoji — return to belt
        heldCard.ReturnToBelt();
        heldCard = null;
    }
}