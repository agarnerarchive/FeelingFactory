using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DraggablePhrase : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [HideInInspector] public string PhraseText;
    [HideInInspector] public ConveyorBelt OwnerBelt;
    [HideInInspector] public RectTransform BeltContainer;
    [HideInInspector] public bool IsDragging { get; private set; }

    [Header("Visual Feedback")]
    public float dragScaleMultiplier = 1.08f;
    public float snapBackDuration = 0.25f;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private RectTransform dragLayer; // a top-level panel to reparent to while dragging

    private Vector2 pointerOffset;
    private Vector3 originalPosition;
    private Transform originalParent;
    private int originalSiblingIndex;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsDragging) return;

        IsDragging = true;

        // Store original parent so we can return if needed
        originalParent = rectTransform.parent;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        originalPosition = rectTransform.position;

        // Find or create a drag layer at the top of the canvas hierarchy
        EnsureDragLayer();

        // Reparent to drag layer so phrase renders on top of everything
        rectTransform.SetParent(dragLayer, true);
        rectTransform.SetAsLastSibling();

        // Calculate offset between pointer and object pivot
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        pointerOffset = rectTransform.anchoredPosition - localPoint;

        // Visual feedback
        canvasGroup.blocksRaycasts = false; // allows drop target to receive raycast
        rectTransform.localScale = Vector3.one * dragScaleMultiplier;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        rectTransform.anchoredPosition = localPoint + pointerOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsDragging) return;

        canvasGroup.blocksRaycasts = true;
        rectTransform.localScale = Vector3.one;

        // Check if we're over a valid drop target
        EmojiTarget dropTarget = GetDropTarget(eventData);

        if (dropTarget != null)
        {
            // Hand off to the target — it will call GameManager and handle outcome
            dropTarget.ReceiveDrop(this);
        }
        else
        {
            // Not dropped on target — return to belt
            ReturnToBelt();
        }

        IsDragging = false;
    }

    private EmojiTarget GetDropTarget(PointerEventData eventData)
    {
        // Walk up the raycast results to find an EmojiTarget
        foreach (var result in eventData.hovered)
        {
            EmojiTarget target = result.GetComponent<EmojiTarget>();
            if (target != null) return target;
        }
        return null;
    }

    public void ReturnToBelt()
    {
        IsDragging = false;
        rectTransform.SetParent(BeltContainer, true);
        rectTransform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.localScale = Vector3.one;

        // Animate snap back to belt center X
        Vector2 targetPos = new Vector2(0, rectTransform.anchoredPosition.y);
        rectTransform.anchoredPosition = targetPos;
    }

    public void DestroyPhrase()
    {
        Destroy(gameObject, 0.05f);
    }

    private void EnsureDragLayer()
    {
        // Look for an existing DragLayer object in the root canvas
        Transform root = rootCanvas.transform;
        Transform existing = root.Find("DragLayer");
        if (existing != null)
        {
            dragLayer = existing.GetComponent<RectTransform>();
        }
        else
        {
            GameObject layer = new GameObject("DragLayer", typeof(RectTransform));
            dragLayer = layer.GetComponent<RectTransform>();
            dragLayer.SetParent(root, false);
            dragLayer.anchorMin = Vector2.zero;
            dragLayer.anchorMax = Vector2.one;
            dragLayer.offsetMin = Vector2.zero;
            dragLayer.offsetMax = Vector2.zero;
            dragLayer.SetAsLastSibling();

            // Add a transparent raycaster blocker — NOT blocking clicks, just organising
            CanvasGroup cg = layer.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }
}