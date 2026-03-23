using UnityEngine;
using TMPro;
using System.Collections;

public class SquareBreathing2D : MonoBehaviour
{
    [Header("UI Elements")]
    public SpriteRenderer progressRing;
    public SpriteRenderer ballSprite;
    public TMP_Text instructionText;

    [Header("Audio")]
    public AudioSource chimeSource;

    [Header("Outro")]
    public IntroPanel introPanel;

    [Header("Phase Durations (seconds)")]
    public float inhaleDuration = 4f;
    public float holdInDuration = 4f;
    public float exhaleDuration = 4f;
    public float holdOutDuration = 4f;

    [Header("Settings")]
    //public float minScale = 0.24f;
    //public float maxScale = 0.3f;
    public int totalCycles = 3;
    public Color inhaleColor = new Color(0.2f, 0.6f, 1f);
    public Color holdColor = new Color(1f, 0.3f, 0.3f);
    public Color completeColor = new Color(0.2f, 0.9f, 0.4f);

    private float timer = 0f;
    private bool isHoldingButton = false;
    private bool sessionStarted = false;
    private bool sessionComplete = false;
    private int completedCycles = 0;

    private enum BreathState { Inhale, HoldIn, Exhale, HoldOut }
    private BreathState currentState = BreathState.Inhale;
    public Animator animator;
    public float outroDelay = 1f;   // pause after final positive emoji before outro fires

    // ── Called by the button's PointerDown event ──────────────────────────────
    public void OnBreathButtonDown()
    {
        if (sessionComplete) return;
        isHoldingButton = true;
        sessionStarted = true;
    }

    // ── Called by the button's PointerUp / PointerExit event ─────────────────
    public void OnBreathButtonUp()
    {
        isHoldingButton = false;
    }

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        ResetToIdle();
        animator.SetBool("idle", true);
    }

    void Update()
    {
        if (sessionComplete) return;

        if (isHoldingButton)
        {
            RunBreathingStep();
        }
        else
        {
            // Button not held — tell the user to keep holding if mid-session
            if (sessionStarted)
                instructionText.text = "Hold to continue...";
        }
    }

    // ── Core breathing logic ──────────────────────────────────────────────────

    void RunBreathingStep()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case BreathState.Inhale:
                //UpdateVisuals(inhaleColor,
                              //Mathf.Lerp(minScale, maxScale, timer / inhaleDuration),
                              //timer / inhaleDuration);
                instructionText.text = $"Breathe In... ({completedCycles + 1}/{totalCycles})";
                if (timer >= inhaleDuration) SwitchState(BreathState.HoldIn);
                animator.SetBool("Idle", false);
                animator.SetBool("Inhaling", true);
                break;

            case BreathState.HoldIn:
                //UpdateVisuals(holdColor, maxScale, timer / holdInDuration);
                instructionText.text = $"Hold... ({completedCycles + 1}/{totalCycles})";
                if (timer >= holdInDuration) SwitchState(BreathState.Exhale);
                break;

            case BreathState.Exhale:
                //UpdateVisuals(inhaleColor,
                              //Mathf.Lerp(maxScale, minScale, timer / exhaleDuration),
                              //timer / exhaleDuration);
                instructionText.text = $"Breathe Out... ({completedCycles + 1}/{totalCycles})";
                if (timer >= exhaleDuration) SwitchState(BreathState.HoldOut);
                animator.SetBool("Inhaling", false);
                animator.SetBool("Exhaling", true);
                break;

            case BreathState.HoldOut:
                animator.SetBool("Exhaling", false);
                animator.SetBool("Idle", true);
                //UpdateVisuals(holdColor, minScale, timer / holdOutDuration);
                instructionText.text = $"Wait... ({completedCycles + 1}/{totalCycles})";
                if (timer >= holdOutDuration) FinishCycle();
                break;
        }
    }

    // ── State helpers ─────────────────────────────────────────────────────────

    void SwitchState(BreathState next)
    {
        timer = 0f;
        currentState = next;
        PlayChime();
    }

    void FinishCycle()
    {
        completedCycles++;

        if (completedCycles >= totalCycles)
            TriggerOutro();
        else
            SwitchState(BreathState.Inhale);
    }

    // ── Visuals ───────────────────────────────────────────────────────────────

    void UpdateVisuals(Color ringColor, float ballScale, float progress)
    {
        // Ball grows/shrinks
       // ballSprite.transform.localScale = new Vector3(ballScale, ballScale, 1f);

        // Ring expands outward as the phase progresses
        float ringScale = ballScale + (progress * 0.8f);
        //progressRing.transform.localScale = new Vector3(ringScale, ringScale, 1f);
        //progressRing.color = ringColor;
    }

    // ── Session end ───────────────────────────────────────────────────────────

    void TriggerOutro()
    {
        sessionComplete = true;
        isHoldingButton = false;
        timer = 0f;

        instructionText.text = "Well done!";
        OutroSequence();
    }
        //ballSprite.transform.localScale     = new Vector3(minScale, minScale, 1f);
        //progressRing.transform.localScale   = new Vector3(minScale + 0.8f, minScale + 0.8f, 1f);
        //progressRing.color = completeColor;

        IEnumerator OutroSequence()
{
    FindFirstObjectByType<ConveyorBelt>()?.SetRunning(false);
    FindFirstObjectByType<ConveyorBelt>()?.ClearAllCards();

    yield return new WaitForSeconds(outroDelay);

    IntroPanel introPanel = FindFirstObjectByType<IntroPanel>(FindObjectsInactive.Include);
    if (introPanel != null)
    {
        // Activate the GameObject first, then call StartOutro
        introPanel.gameObject.SetActive(true);
        yield return null; // wait one frame for activation to complete
        introPanel.StartOutro();
    }
    else
    {
        Debug.LogError("EmojiCharacter: No IntroPanel found in scene!");
    }
}
    // ── Idle / reset ──────────────────────────────────────────────────────────

    void ResetToIdle()
    {
        timer = 0f;
        currentState = BreathState.Inhale;
        instructionText.text = "Hold the button";

        //ballSprite.transform.localScale     = new Vector3(minScale, minScale, 1f);
        //progressRing.transform.localScale   = new Vector3(minScale, minScale, 1f);
        //progressRing.color = Color.gray;
    }

    public void RestartSession()
    {
        completedCycles = 0;
        sessionComplete = false;
        sessionStarted = false;
        ResetToIdle();
    }

    // ── Audio ─────────────────────────────────────────────────────────────────

    void PlayChime()
    {
        if (chimeSource != null) chimeSource.Play();
    }
}