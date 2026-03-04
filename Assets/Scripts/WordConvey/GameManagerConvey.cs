// Assets/Scripts/GameManager.cs
using UnityEngine;

public class GameManagerConvey : MonoBehaviour
{
    public static GameManagerConvey Instance { get; private set; }
    public ConveyorBelt conveyor;

    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

    public void NextRound()
    {
        conveyor.cardSpeed     += 0.3f;
        conveyor.spawnInterval  = Mathf.Max(1.2f, conveyor.spawnInterval - 0.2f);
    }
}