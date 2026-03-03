// Assets/Scripts/GameManager.cs
using UnityEngine;
using TMPro;

public class GameManagerConvey : MonoBehaviour
{
    public static GameManagerConvey Instance { get; private set; }
    public ConveyorBelt conveyor;
    public TextMesh roundLabel;   // TextMesh (3D), not UI
    public TextMesh scoreLabel;
    public int score = 0, round = 1;

    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }
    void Start()  => UpdateUI();

    public void NextRound()
    {
        round++;
        score += 100;
        conveyor.cardSpeed     += 0.3f;
        conveyor.spawnInterval  = Mathf.Max(1.2f, conveyor.spawnInterval - 0.2f);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (roundLabel) roundLabel.text = $"Round {round}";
        if (scoreLabel) scoreLabel.text = $"Score: {score}";
    }
}