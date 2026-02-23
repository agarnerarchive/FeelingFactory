using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ConveyorGame/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Emoji")]
    public Sprite emojiSprite;

    [Header("Phrases")]
    [Tooltip("The phrase the player must drag onto the emoji")]
    public string correctPhrase;

    [Tooltip("All phrases to show on the belt, including the correct one")]
    public string[] allPhrases;

    [Header("Settings")]
    [Tooltip("How many correct drops to complete this level")]
    public int correctDropsRequired = 3;

    [Tooltip("Belt scroll speed for this level")]
    public float beltSpeed = 80f;
}