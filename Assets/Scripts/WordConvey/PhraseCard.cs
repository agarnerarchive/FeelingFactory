// Assets/Scripts/PhraseCard.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PhraseCard : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro label;          // TextMeshPro (3D), NOT TextMeshProUGUI

    [Header("Colors")]
    public Color neutralColor  = new Color(0.95f, 0.95f, 0.95f);
    public Color goodColor     = new Color(0.6f,  1f,    0.65f);
    public Color badColor      = new Color(1f,    0.55f, 0.55f);

    // State
    [HideInInspector] public bool isGood;
    [HideInInspector] public bool isDragging;

    private SpriteRenderer sr;
    private ConveyorBelt conveyor;
    private Vector3 dragOffset;
    private Vector3 homePosition;          // position when not being dragged
    private bool isReturning = false;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Setup(PhraseData.Phrase phrase, ConveyorBelt belt)
    {
        isGood    = phrase.isGood;
        conveyor  = belt;
        label.text = phrase.text;
        sr.color  = neutralColor;
        homePosition = transform.position;

        // Sorting order so cards draw above belt
        sr.sortingOrder = 2;
        label.sortingOrder = 3;
    }

    // Called by InputManager
    public void BeginDrag(Vector3 worldPos)
    {
        isDragging  = true;
        isReturning = false;
        dragOffset  = transform.position - worldPos;
        homePosition = transform.position;
        sr.sortingOrder = 10;  // draw on top while dragging
        label.sortingOrder = 11;
        conveyor?.PauseCard(this);
    }

    public void Drag(Vector3 worldPos)
    {
        transform.position = new Vector3(
            worldPos.x + dragOffset.x,
            worldPos.y + dragOffset.y,
            transform.position.z);
    }

    public void EndDrag()
    {
        isDragging = false;
        sr.sortingOrder = 2;
        label.sortingOrder = 3;
    }

    // Called when card was NOT accepted — slide back to belt
    public void ReturnToBelt()
    {
        isReturning = true;
        StartCoroutine(SlideBack());
    }

    System.Collections.IEnumerator SlideBack()
    {
        Vector3 start = transform.position;
        float elapsed = 0f, duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, homePosition, elapsed / duration);
            yield return null;
        }
        transform.position = homePosition;
        isReturning = false;
        conveyor?.ResumeCard(this);
    }

    // Called by ConveyorBelt to move card downward
    public void MoveDown(float speed)
    {
        if (!isDragging && !isReturning)
            transform.position += Vector3.down * speed * Time.deltaTime;
    }
}
