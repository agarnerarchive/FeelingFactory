using UnityEngine;
using System.Collections.Generic;

public enum BubbleType
{
    PositivePhrase,
    NegativePhrase,
    PositiveEmoji,
    NegativeEmoji
}

[CreateAssetMenu(fileName = "BubbleContentData", menuName = "EmojiPop/BubbleContentData")]
public class BubbleData : ScriptableObject
{
    [Header("Positive Phrases")]
    public List<string> positivePhrases = new List<string>
    {
        "You're amazing!", "Keep going!", "Love you!", "You're kind!", "Believe in yourself!"
    };

    [Header("Negative Phrases")]
    public List<string> negativePhrases = new List<string>
    {
        "You're bad!", "Give up!", "You're wrong!", "Nobody likes you!", "Stop trying!"
    };

    [Header("Positive Emojis (sprites)")]
    public List<Sprite> positiveEmojiSprites;   // 😊 ❤️ ⭐ 👍 🌟

    [Header("Negative Emojis (sprites)")]
    public List<Sprite> negativeEmojiSprites;   // 😠 💢 👎 🤬 💀

    [Header("Bubble Colors")]
    public Color positiveColor = new Color(0.6f, 1f, 0.6f, 0.85f);
    public Color negativeColor = new Color(1f, 0.5f, 0.5f, 0.85f);

    [Header("Scoring")]
    public int positiveHitScore = 10;   // positive bubbles reaching emoji
    public int negativePoppedScore = 5; // player pops a negative bubble

    public string GetRandomPositivePhrase() =>
        positivePhrases[Random.Range(0, positivePhrases.Count)];

    public string GetRandomNegativePhrase() =>
        negativePhrases[Random.Range(0, negativePhrases.Count)];

    public Sprite GetRandomPositiveEmojiSprite() =>
        positiveEmojiSprites.Count > 0
            ? positiveEmojiSprites[Random.Range(0, positiveEmojiSprites.Count)]
            : null;

    public Sprite GetRandomNegativeEmojiSprite() =>
        negativeEmojiSprites.Count > 0
            ? negativeEmojiSprites[Random.Range(0, negativeEmojiSprites.Count)]
            : null;
}