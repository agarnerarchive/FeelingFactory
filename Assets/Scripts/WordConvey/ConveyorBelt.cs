// Assets/Scripts/ConveyorBelt.cs
using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Data")]
    public PhraseData phraseDatabase;
    public GameObject phraseCardPrefab;

    [Header("Points")]
    public Transform spawnPoint;
    public Transform despawnPoint;

    [Header("Timing & Speed")]
    public float cardSpeed     = 2f;    // world units per second
    public float spawnInterval = 2.5f;

    [Header("Card Size")]
    public Vector2 cardSize = new Vector2(2f, 0.6f);

    private List<PhraseCard> activeCards  = new List<PhraseCard>();
    private List<PhraseCard> pausedCards  = new List<PhraseCard>();
    private float spawnTimer = 0.5f;
    private bool  isRunning  = true;

    void Update()
    {
        if (!isRunning) return;

        // Move all non-paused cards downward
        foreach (var card in activeCards)
        {
            if (card == null || pausedCards.Contains(card)) continue;
            card.MoveDown(cardSpeed);
        }

        // Despawn cards that passed the bottom
        activeCards.RemoveAll(card => {
            if (card == null) return true;
            if (card.transform.position.y < despawnPoint.position.y)
            {
                Destroy(card.gameObject);
                return true;
            }
            return false;
        });

        // Spawn timer
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnCard();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnCard()
    {
        var phrases = phraseDatabase.phrases;
        var phrase  = phrases[Random.Range(0, phrases.Length)];

        GameObject go = Instantiate(phraseCardPrefab, spawnPoint.position, Quaternion.identity);

        // Size the collider and sprite to match cardSize
        go.transform.localScale = new Vector3(cardSize.x, cardSize.y, 1f);

        PhraseCard card = go.GetComponent<PhraseCard>();
        card.Setup(phrase, this);
        activeCards.Add(card);
    }

    public void PauseCard(PhraseCard card)  => pausedCards.Add(card);
    public void ResumeCard(PhraseCard card) => pausedCards.Remove(card);
    public void SetRunning(bool running)    => isRunning = running;
}