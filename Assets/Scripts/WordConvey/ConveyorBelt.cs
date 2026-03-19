// Assets/Scripts/ConveyorBelt.cs
using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject phraseCardPrefab;

    [Header("Points")]
    public Transform spawnPoint;
    public Transform despawnPoint;

    [Header("Settings")]
    public float cardSpeed     = 2f;
    public float spawnInterval = 2.5f;
    public Vector2 cardSize    = new Vector2(2f, 0.6f);

    private List<PhraseCard> activeCards  = new List<PhraseCard>();
    private List<PhraseCard> pausedCards  = new List<PhraseCard>();
    private List<PhraseCard> cardsToRemove = new List<PhraseCard>();
    private EmojiData        currentData;
    private float            spawnTimer   = 0.5f;
    private bool             isRunning    = true;

    void Update()
    {
        if (!isRunning || currentData == null) return;

        // Move cards — iterate a copy to avoid modification issues
        foreach (var card in new List<PhraseCard>(activeCards))
        {
            if (card == null || pausedCards.Contains(card)) continue;
            card.MoveDown(cardSpeed);
        }

        // Collect cards to remove separately
        cardsToRemove.Clear();
        foreach (var card in activeCards)
        {
            if (card == null)
            {
                cardsToRemove.Add(card);
                continue;
            }
            if (card.transform.position.y < despawnPoint.position.y)
            {
                Destroy(card.gameObject);
                cardsToRemove.Add(card);
            }
        }

        // Now safe to remove
        foreach (var card in cardsToRemove)
            activeCards.Remove(card);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnCard();
            spawnTimer = spawnInterval;
        }
    }

    public void SetEmojiData(EmojiData data)
    {
        currentData = data;
        ClearAllCards();
        spawnTimer = 0.5f;
    }

    void SpawnCard()
    {
        if (phraseCardPrefab == null)
        {
            Debug.LogError("ConveyorBelt: Phrase Card Prefab is not assigned!");
            SetRunning(false);
            return;
        }

        bool     spawnGood = Random.value > 0.5f;
        string[] pool      = spawnGood ? currentData.goodPhrases : currentData.badPhrases;

        if (pool == null || pool.Length == 0) return;

        string text = pool[Random.Range(0, pool.Length)];

        GameObject go = Instantiate(phraseCardPrefab, spawnPoint.position, Quaternion.identity);
        go.transform.localScale = new Vector3(cardSize.x, cardSize.y, 1f);

        PhraseCard card = go.GetComponent<PhraseCard>();
        card.SetupDirect(text, spawnGood, this);
        activeCards.Add(card);
    }

    public void ClearAllCards()
    {
        foreach (var card in activeCards)
            if (card != null) Destroy(card.gameObject);
        activeCards.Clear();
        pausedCards.Clear();
        cardsToRemove.Clear();
    }

    public void PauseCard(PhraseCard card)  => pausedCards.Add(card);
    public void ResumeCard(PhraseCard card) => pausedCards.Remove(card);
    public void SetRunning(bool running)    => isRunning = running;
}