// DraggablePhrase.cs
// Attach to your PhrasePrefab (which must have a CanvasGroup component).
 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
 
[RequireComponent(typeof(CanvasGroup))]
public class DraggablePhrase : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public TextMeshProUGUI phraseLabel;   // Text display
    public Image backgroundImage;          // Optional: tint on drag
 
    // Private state
    private string phrase;
    private bool isCorrect;
    private float speed;
    private RectTransform despawnPoint;
    private ConveyorBelt belt;
    private bool isMoving  = true;
    private bool isDragging = false;
    private Vector3 beltPosition;         // Last belt position (for return)
 
    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
 
    // ── Initialisation ────────────────────────────────────────────────────────
    public void Initialise(string p, bool correct, float spd,
                            RectTransform despawn, ConveyorBelt parentBelt)
    {
        phrase        = p;
        isCorrect     = correct;
        speed         = spd;
        despawnPoint  = despawn;
        belt          = parentBelt;
 
        if (phraseLabel != null) phraseLabel.text = phrase;
 
        rectTransform = GetComponent<RectTransform>();
        canvasGroup   = GetComponent<CanvasGroup>();
        rootCanvas    = GetComponentInParent<Canvas>();
    }
 
    // ── Movement ──────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!isMoving || isDragging) return;
 
        // Move downward in world space
        transform.position += Vector3.down * speed * Time.deltaTime;
 
        // If below despawn line, loop back to top of belt
        if (despawnPoint != null &&
            transform.position.y < despawnPoint.position.y)
        {
            belt.RespawnPhrase(this);
        }
    }
 
    public void ResumeMoving()
    {
        isMoving = true;
    }
 
    public void StopMoving()
    {
        isMoving = false;
        isDragging = false;
    }
 
    // ── Drag Handlers ─────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        beltPosition = transform.position; // Remember where we were on belt
        isDragging   = true;
        isMoving     = false;
 
        // Let drop targets receive pointer events
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
 
        // Bring this card to the front
        transform.SetAsLastSibling();
    }
 
    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvas == null) return;
 
        // Move with pointer in canvas local space
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPos);
 
        rectTransform.localPosition = localPos;
    }
 
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
 
        // If not dropped on a valid target, resume belt movement
        // (EmojiController.OnDrop handles the valid-target case)
        isMoving = true;
    }
 
    // ── Result Handling ───────────────────────────────────────────────────────
    /// Called by EmojiController when dropped in wrong place.
    public void ReturnToBelt()
    {
        transform.position = beltPosition;
        isMoving = true;
    }
 
    // ── Getters ───────────────────────────────────────────────────────────────
    public bool IsCorrect()  => isCorrect;
    public string GetPhrase() => phrase;
}


