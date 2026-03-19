// Assets/Scripts/PhraseCard.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PhraseCard : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro label;

    [Header("Colors")]
    public Color neutralColor = new Color(0.95f, 0.95f, 0.95f);

    [HideInInspector] public bool isGood;

    private SpriteRenderer sr;
    private ConveyorBelt   conveyor;
    private Vector3        dragOffset;
    private Vector3        homePosition;
    private bool           isReturning = false;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    // Called by ConveyorBelt with raw text + isGood flag
    public void SetupDirect(string text, bool good, ConveyorBelt belt)
    {
        isGood       = good;
        conveyor     = belt;
        label.text   = text;
        sr.color     = neutralColor;
        homePosition = transform.position;
        sr.sortingOrder    = 2;
        label.sortingOrder = 3;
    }

    public void BeginDrag(Vector3 worldPos)
    {
        isReturning  = false;
        dragOffset   = transform.position - worldPos;
        homePosition = transform.position;
        sr.sortingOrder    = 10;
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
        sr.sortingOrder    = 2;
        label.sortingOrder = 3;
    }

    public void ReturnToBelt()
    {
        isReturning = true;
        StartCoroutine(SlideBack());
    }

    System.Collections.IEnumerator SlideBack()
    {
        Vector3 start   = transform.position;
        float   elapsed = 0f, duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, homePosition, elapsed / duration);
            yield return null;
        }
        transform.position = homePosition;
        isReturning        = false;
        conveyor?.ResumeCard(this);
    }

    public void MoveDown(float speed)
    {
        if (!isReturning)
            transform.position += Vector3.down * speed * Time.deltaTime;
    }
}