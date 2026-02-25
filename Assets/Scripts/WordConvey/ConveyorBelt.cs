// ConveyorBelt.cs
// Attach to an empty GameObject named "ConveyorBelt".
 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class ConveyorBelt : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject phrasePrefab;        // Your DraggablePhrase prefab
 
    [Header("Spawn Points (assign RectTransform children)")]
    public RectTransform spawnPoint;       // Top of conveyor belt
    public RectTransform despawnPoint;     // Below bottom of screen
 
    [Header("Settings")]
    public float spawnInterval = 2.5f;    // Seconds between spawns
    public float phraseSpeed = 120f;       // Pixels per second (UI space)
    [Range(0f, 1f)]
    public float correctSpawnChance = 0.3f; // 30% chance of spawning correct phrase
 
    // Private state
    private string currentCorrectPhrase;
    private string[] currentIncorrectPhrases;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private List<DraggablePhrase> activePhrazes = new List<DraggablePhrase>();
 
    // ── Public API ────────────────────────────────────────────────────────────
    public void SetPhrases(string correct, string[] incorrect)
    {
        currentCorrectPhrase  = correct;
        currentIncorrectPhrases = incorrect;
    }
 
    public void StartSpawning()
    {
        isSpawning = true;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }
 
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }
 
    public void ClearAllPhrases()
    {
        foreach (var p in activePhrazes)
            if (p != null) Destroy(p.gameObject);
        activePhrazes.Clear();
    }
 
    public void RemoveFromList(DraggablePhrase phrase)
    {
        activePhrazes.Remove(phrase);
    }
 
    public void RespawnPhrase(DraggablePhrase phrase)
    {
        // Move phrase back to top of belt
        phrase.transform.position = spawnPoint.position;
        phrase.ResumeMoving();
    }
 
    // ── Private ───────────────────────────────────────────────────────────────
    private IEnumerator SpawnLoop()
    {
        // Stagger the first spawn slightly
        yield return new WaitForSeconds(0.5f);
 
        while (isSpawning)
        {
            SpawnPhrase();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
 
    private void SpawnPhrase()
    {
        if (phrasePrefab == null || spawnPoint == null) return;
        if (currentCorrectPhrase == null) return;
 
        bool isCorrect = Random.value < correctSpawnChance;
        string phraseText;
 
        if (isCorrect)
        {
            phraseText = currentCorrectPhrase;
        }
        else
        {
            if (currentIncorrectPhrases == null || currentIncorrectPhrases.Length == 0)
            {
                phraseText = currentCorrectPhrase; // fallback
                isCorrect = true;
            }
            else
            {
                phraseText = currentIncorrectPhrases[
                    Random.Range(0, currentIncorrectPhrases.Length)];
            }
        }
 
        // Instantiate as a child of this transform (which is inside the Canvas)
        GameObject obj = Instantiate(phrasePrefab, transform);
        obj.transform.position = spawnPoint.position;
 
        DraggablePhrase dp = obj.GetComponent<DraggablePhrase>();
        if (dp != null)
        {
            dp.Initialise(phraseText, isCorrect, phraseSpeed, despawnPoint, this);
            activePhrazes.Add(dp);
        }
    }
}

