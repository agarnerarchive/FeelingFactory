// Assets/Scripts/EmojiData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "EmojiData", menuName = "Game/Emoji Data")]
public class EmojiData : ScriptableObject
{
    [Header("Sprites")]
    public Sprite negativeSprite;
    public Sprite positiveSprite;

    [Header("Phrases")]
    public string[] goodPhrases;
    public string[] badPhrases;

    [Header("Animations")]
    public AnimationClip idleClip;
    public AnimationClip correctClip;
    public AnimationClip wrongClip;
}