using UnityEngine;
using System.Collections;
 
public class GameManagerConvey : MonoBehaviour
{
    public static GameManagerConvey Instance { get; private set; }
 
    [Header("Emoji Sequence (drag 4 EmojiData assets here)")]
    public EmojiData[] emojiSequence;
 
    [Header("Scene References")]
    public EmojiController emojiController;
    public ConveyorBelt conveyorBelt;
    public CoverController coverController;
    public ProgressBar progressBar;
    public ScreenShake screenShake;
 
    private int currentEmojiIndex = 0;
    private bool transitionActive = false;
 
    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
 
    private void Start()
    {
        LoadEmojiAtIndex(0);
    }
 
    // ── Public API (called from other scripts) ────────────────────────────────
    public void LoadEmojiAtIndex(int index)
    {
        if (index >= emojiSequence.Length)
        {
            Debug.Log("[GameManager] All emojis complete! Show win screen.");
            return;
        }
 
        currentEmojiIndex = index;
        EmojiData data = emojiSequence[index];
 
        emojiController.SetupEmoji(data);
        progressBar.Initialise(data.requiredScore);
        conveyorBelt.SetPhrases(data.correctPhrase, data.incorrectPhrases);
        conveyorBelt.StartSpawning();
    }
 
    /// Called by ProgressBar when target score is reached.
    public void OnProgressComplete()
    {
        if (transitionActive) return;
        transitionActive = true;
        conveyorBelt.StopSpawning();
        StartCoroutine(EmojiTransitionSequence());
    }
 
    // ── Private Coroutines ────────────────────────────────────────────────────
    private IEnumerator EmojiTransitionSequence()
    {
        // 1. Wait 2 seconds before anything happens
        yield return new WaitForSeconds(2f);
 
        // 2. Lower the cover and shake screen
        coverController.LowerCover();
        screenShake.Shake(0.5f, 0.3f);
 
        // 3. Play transition sound for the current emoji
        EmojiData currentData = emojiSequence[currentEmojiIndex];
        if (currentData.transitionSound != null)
            AudioSource.PlayClipAtPoint(currentData.transitionSound,
                Camera.main.transform.position);
 
        // 4. Wait for cover to fully lower
        yield return new WaitForSeconds(coverController.LowerDuration);
 
        // 5. Destroy current emoji and spawn next (while hidden behind cover)
        emojiController.HideCurrentEmoji();
        int nextIndex = currentEmojiIndex + 1;
        if (nextIndex < emojiSequence.Length)
            emojiController.SpawnEmoji(emojiSequence[nextIndex]);
 
        // Short pause so spawn completes before reveal
        yield return new WaitForSeconds(0.3f);
 
        // 6. Raise the cover to reveal new emoji
        coverController.RaiseCover();
        yield return new WaitForSeconds(coverController.RaiseDuration);
 
        // 7. Set up game state for new emoji
        transitionActive = false;
        conveyorBelt.ClearAllPhrases();
 
        if (nextIndex < emojiSequence.Length)
        {
            currentEmojiIndex = nextIndex;
            progressBar.Initialise(emojiSequence[nextIndex].requiredScore);
            conveyorBelt.SetPhrases(emojiSequence[nextIndex].correctPhrase,
                                    emojiSequence[nextIndex].incorrectPhrases);
            conveyorBelt.StartSpawning();
        }
        else
        {
            Debug.Log("[GameManager] Game Complete!");
        }
    }
}

