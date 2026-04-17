using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Prefab & Data")]
    public GameObject bubblePrefab;
    public BubbleData bubbleData;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public int maxBubblesOnScreen = 10;

    [Header("Type Weights")]
    [Range(1, 10)] public int positiveWeight = 4;
    [Range(1, 10)] public int negativeWeight = 3;

    private Transform emojiTarget;
    private float spawnTimer;
    private int currentBubbleCount;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        GameObject emojiObj = GameObject.FindGameObjectWithTag("MainEmoji");
        if (emojiObj != null)
            emojiTarget = emojiObj.transform;
    }

    private void Update()
    {
        if (!GameManagerPop.Instance.isGameActive) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && currentBubbleCount < maxBubblesOnScreen)
        {
            spawnTimer = 0f;
            SpawnBubble();
        }
    }

    private void SpawnBubble()
{
    if (bubblePrefab == null) { Debug.LogError("BubbleSpawner: bubblePrefab is null!"); return; }
    if (bubbleData == null)   { Debug.LogError("BubbleSpawner: bubbleData is null!");   return; }
    if (emojiTarget == null)  { Debug.LogError("BubbleSpawner: emojiTarget is null — is the MainEmoji tag set?"); return; }

    // ... rest of SpawnBubble


        Vector2 spawnPos = GetScreenEdgeSpawnPoint();

        GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        Bubble bubble = obj.GetComponent<Bubble>();
        if (bubble == null) { Destroy(obj); return; }

        Vector2 target = emojiTarget != null ? (Vector2)emojiTarget.position : Vector2.zero;
        bubble.Initialize(PickRandomType(), bubbleData, target);

        currentBubbleCount++;
        obj.AddComponent<BubbleDestroyNotifier>().spawner = this;
    }

    // Picks a random point just outside the visible screen edges
    private Vector2 GetScreenEdgeSpawnPoint()
    {
        float camH = mainCam.orthographicSize;
        float camW = mainCam.orthographicSize * mainCam.aspect;
        float padding = 0.8f; // how far outside the edge to spawn

        // Pick one of 4 edges randomly, then a random position along that edge
        int edge = Random.Range(0, 4);
        return edge switch
        {
            0 => new Vector2(Random.Range(-camW, camW),  camH + padding), // top
            1 => new Vector2(Random.Range(-camW, camW), -camH - padding), // bottom
            2 => new Vector2(-camW - padding, Random.Range(-camH, camH)), // left
            _ => new Vector2( camW + padding, Random.Range(-camH, camH))  // right
        };
    }

    private BubbleType PickRandomType()
    {
        int total = positiveWeight * 2 + negativeWeight * 2;
        int roll  = Random.Range(0, total);

        if (roll < positiveWeight)             return BubbleType.PositivePhrase;
        roll -= positiveWeight;
        if (roll < positiveWeight)             return BubbleType.PositiveEmoji;
        roll -= positiveWeight;
        if (roll < negativeWeight)             return BubbleType.NegativePhrase;
        return BubbleType.NegativeEmoji;
    }

    public void OnBubbleDestroyed() =>
        currentBubbleCount = Mathf.Max(0, currentBubbleCount - 1);
}

public class BubbleDestroyNotifier : MonoBehaviour
{
    public BubbleSpawner spawner;
    private void OnDestroy() { if (spawner != null) spawner.OnBubbleDestroyed(); }
}