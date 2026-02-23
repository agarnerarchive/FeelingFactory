using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EmojiTarget : MonoBehaviour
{
    [Header("References")]
    public Image emojiImage; // same Image as GameManager uses
    public ParticleSystem correctEffect;
    public ParticleSystem wrongEffect;

    [Header("Visual Feedback")]
    public Color correctFlashColor = new Color(0.5f, 1f, 0.5f);
    public Color wrongFlashColor = new Color(1f, 0.4f, 0.4f);
    public float flashDuration = 0.35f;

    private Color originalColor;

    void Awake()
    {
        if (emojiImage == null)
            emojiImage = GetComponent<Image>();
        originalColor = emojiImage.color;
    }

    public void ReceiveDrop(DraggablePhrase phrase)
    {
        bool correct = ConveyorGameManager.Instance.EvaluateDrop(phrase.PhraseText);

        if (correct)
        {
            phrase.DestroyPhrase();
            StartCoroutine(FlashColor(correctFlashColor));
            if (correctEffect != null) correctEffect.Play();
        }
        else
        {
            phrase.ReturnToBelt();
            StartCoroutine(FlashColor(wrongFlashColor));
            if (wrongEffect != null) wrongEffect.Play();
        }
    }

    private IEnumerator FlashColor(Color flash)
    {
        emojiImage.color = flash;
        yield return new WaitForSeconds(flashDuration);
        emojiImage.color = originalColor;
    }
}