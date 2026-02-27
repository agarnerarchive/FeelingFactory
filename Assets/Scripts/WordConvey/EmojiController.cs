// EmojiController.cs
// Attach to the EmojiZone panel (the drop target on the left side).
 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
 
public class EmojiController : MonoBehaviour, IDropHandler
{
    [Header("Emoji Display")]
    public Image emojiImage;              // Image component showing the emoji sprite
    public Animator emojiAnimator;        // Animator on the emoji image
 
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip incorrectSFX;
 
    private EmojiData currentData;
    private bool acceptingDrops = true;
 
    // ── Setup ─────────────────────────────────────────────────────────────────
    /// Called at the START of a new emoji round.
    public void SetupEmoji(EmojiData data)
    {
        currentData  = data;
        acceptingDrops = true;
 
        if (emojiImage != null)
        {
            emojiImage.sprite  = data.emojiSprite;
            emojiImage.enabled = true;
        }
    }
 
    /// Called mid-transition to swap sprite (while hidden behind cover).
    public void SpawnEmoji(EmojiData data)
    {
        currentData = data;
        if (emojiImage != null)
        {
            emojiImage.sprite  = data.emojiSprite;
            emojiImage.enabled = true;
        }
        acceptingDrops = true;
    }
 
    /// Called to hide the emoji (while cover is lowering).
    public void HideCurrentEmoji()
    {
        acceptingDrops = false;
        if (emojiImage != null)
            emojiImage.enabled = false;
    }
 
    // ── Drop Handler ──────────────────────────────────────────────────────────
    public void OnDrop(PointerEventData eventData)
    {
        if (!acceptingDrops) return;
 
        // Retrieve the phrase from whatever was dragged
        DraggablePhrase phrase = eventData.pointerDrag?.GetComponent<DraggablePhrase>();
        if (phrase == null) return;
 
        if (phrase.IsCorrect())
            HandleCorrectDrop(phrase);
        else
            HandleIncorrectDrop(phrase);
    }
 
    // ── Private Handlers ──────────────────────────────────────────────────────
    private void HandleCorrectDrop(DraggablePhrase phrase)
    {
        // Stop the phrase moving
        phrase.StopMoving();
 
        // Play success animation on the emoji
        if (emojiAnimator != null)
            emojiAnimator.SetTrigger("Correct");
 
        // Play SFX
        if (correctSFX != null && audioSource != null)
            audioSource.PlayOneShot(correctSFX);
 
        // Remove from belt tracking and destroy phrase card
        GameManagerConvey.Instance.conveyorBelt.RemoveFromList(phrase);
        Destroy(phrase.gameObject);
 
        // Award point
        ProgressBar.Instance.AddScore(1);
    }
 
    private void HandleIncorrectDrop(DraggablePhrase phrase)
    {
        // Screen shake
        GameManagerConvey.Instance.screenShake.Shake(0.35f, 20f);
 
        // Play SFX
        if (incorrectSFX != null && audioSource != null)
            audioSource.PlayOneShot(incorrectSFX);
 
        // Deduct point and return phrase to belt
        ProgressBar.Instance.AddScore(-1);
        phrase.ReturnToBelt();
    }
}

