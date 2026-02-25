// EmojiData.cs
// Create via Right-click → Create → Game → EmojiData in Project window
 
using UnityEngine;
 
[CreateAssetMenu(fileName = "EmojiData", menuName = "Game/EmojiData")]
public class EmojiData : ScriptableObject
{
    [Header("Visuals")]
    public string emojiName;
    public Sprite emojiSprite;           // The emoji image
 
    [Header("Phrases")]
    public string correctPhrase;          // One correct answer
    public string[] incorrectPhrases;     // Wrong answers pool
 
    [Header("Scoring")]
    public int requiredScore = 5;         // Points needed to complete this emoji
 
    [Header("Audio / Animation")]
    public AudioClip transitionSound;     // Played when cover descends
}

