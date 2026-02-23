using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RectTransform that acts as the visible belt window (add a Mask component to this)")]
    public RectTransform beltViewport;

    [Tooltip("Prefab for each phrase item — should have DraggablePhrase + TextMeshProUGUI")]
    public GameObject phrasePrefab;

    [Header("Belt Settings")]
    [Tooltip("Vertical gap between spawned phrases")]
    public float phraseSpacing = 120f;

    [Tooltip("How far above the viewport top phrases spawn")]
    public float spawnOffsetAbove = 60f;

    private string[] currentPhrases;
    private float currentSpeed = 80f;
    private bool running = false;

    private List<RectTransform> activePhrases = new List<RectTransform>();
    private float nextSpawnY;

    // The belt uses a single moving container parented to the viewport
    private RectTransform beltContainer;

    void Awake()
    {
        // Create a container inside the viewport to hold all phrase objects
        GameObject container = new GameObject("BeltContainer", typeof(RectTransform));
        beltContainer = container.GetComponent<RectTransform>();
        beltContainer.SetParent(beltViewport, false);
        beltContainer.anchorMin = Vector2.zero;
        beltContainer.anchorMax = Vector2.one;
        beltContainer.offsetMin = Vector2.zero;
        beltContainer.offsetMax = Vector2.zero;
    }

    public void StartBelt(string[] phrases, float speed)
    {
        StopBelt();
        ClearBelt();

        currentPhrases = phrases;
        currentSpeed = speed;
        running = true;

        // Start spawning from above the top of the viewport
        nextSpawnY = beltViewport.rect.height / 2f + spawnOffsetAbove;

        // Pre-populate the belt so it's not empty at start
        PreFill();
    }

    public void StopBelt()
    {
        running = false;
    }

    void ClearBelt()
    {
        foreach (Transform child in beltContainer)
            Destroy(child.gameObject);
        activePhrases.Clear();
    }

    void PreFill()
    {
        float viewH = beltViewport.rect.height;
        float y = viewH / 2f + spawnOffsetAbove;

        while (y > -(viewH / 2f + phraseSpacing))
        {
            SpawnPhraseAt(y);
            y -= phraseSpacing;
        }
        nextSpawnY = y;
    }

    void Update()
    {
        if (!running) return;

        float delta = currentSpeed * Time.deltaTime;
        float viewH = beltViewport.rect.height;
        float bottomBound = -(viewH / 2f + phraseSpacing);
        float topSpawnPoint = viewH / 2f + spawnOffsetAbove;

        // Move all active (non-dragged) phrases downward
        List<RectTransform> toRemove = new List<RectTransform>();

        foreach (var phrase in activePhrases)
        {
            if (phrase == null)
            {
                toRemove.Add(phrase);
                continue;
            }

            // Only move if not being dragged
            DraggablePhrase dp = phrase.GetComponent<DraggablePhrase>();
            if (dp != null && dp.IsDragging) continue;

            // If phrase was released back to belt, it's owned by beltContainer
            if (phrase.parent == beltContainer)
            {
                phrase.anchoredPosition += Vector2.down * delta;
            }

            // Recycle if off bottom
            if (phrase.anchoredPosition.y < bottomBound)
            {
                toRemove.Add(phrase);
            }
        }

        foreach (var r in toRemove)
        {
            activePhrases.Remove(r);
            if (r != null) Destroy(r.gameObject);
        }

        // Spawn new phrases at top as needed
        nextSpawnY -= delta;
        while (nextSpawnY > bottomBound && 
               (activePhrases.Count == 0 || nextSpawnY > (activePhrases.Count > 0 ? GetHighestY() : topSpawnPoint) - phraseSpacing))
        {
            // Check if the topmost phrase has moved enough to warrant a new spawn
            if (activePhrases.Count == 0 || GetHighestY() <= topSpawnPoint - phraseSpacing)
            {
                SpawnPhraseAt(topSpawnPoint);
            }
            break;
        }

        // Simpler spawn trigger: if gap above top phrase is big enough, spawn
        if (activePhrases.Count == 0 || GetHighestActivePhraseY() < topSpawnPoint - phraseSpacing)
        {
            SpawnPhraseAt(topSpawnPoint);
        }
    }

    float GetHighestActivePhraseY()
    {
        float highest = float.MinValue;
        foreach (var p in activePhrases)
        {
            if (p != null && p.parent == beltContainer)
            {
                if (p.anchoredPosition.y > highest)
                    highest = p.anchoredPosition.y;
            }
        }
        return highest == float.MinValue ? -(beltViewport.rect.height) : highest;
    }

    float GetHighestY()
    {
        float h = float.MinValue;
        foreach (var p in activePhrases)
            if (p != null && p.anchoredPosition.y > h)
                h = p.anchoredPosition.y;
        return h;
    }

    void SpawnPhraseAt(float yPos)
    {
        if (currentPhrases == null || currentPhrases.Length == 0) return;

        string phrase = currentPhrases[Random.Range(0, currentPhrases.Length)];

        GameObject obj = Instantiate(phrasePrefab, beltContainer);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(0, yPos);

        // Set phrase text
        TextMeshProUGUI tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = phrase;

        // Tell the DraggablePhrase what text it holds
        DraggablePhrase dp = obj.GetComponent<DraggablePhrase>();
        if (dp != null)
        {
            dp.PhraseText = phrase;
            dp.OwnerBelt = this;
            dp.BeltContainer = beltContainer;
        }

        activePhrases.Add(rt);
    }

    /// <summary>Called by DraggablePhrase when a drag is cancelled — returns phrase to belt.</summary>
    public void ReturnPhraseToBelt(DraggablePhrase phrase)
    {
        if (phrase == null) return;
        RectTransform rt = phrase.GetComponent<RectTransform>();
        rt.SetParent(beltContainer, true);
        // Snap back to belt's X center
        rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);
    }
}