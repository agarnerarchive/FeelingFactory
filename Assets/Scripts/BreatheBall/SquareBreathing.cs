using UnityEngine;
using TMPro;
using System.Collections;

public class SquareBreathing2D : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text instructionText;

    [Header("Audio")]
    public AudioSource chimeSource;

    [Header("Animator")]
    public Animator animator;

    [Header("Phase Durations (seconds)")]
    public float inhaleDuration  = 4f;
    public float holdInDuration  = 4f;
    public float exhaleDuration  = 4f;
    public float holdOutDuration = 4f;

    [Header("Settings")]
    public int   totalCycles = 3;
    public float outroDelay  = 1f;

    // ── Private state ──────────────────────────────────────────────────────────

    private float timer           = 0f;
    private bool  isHoldingButton = false;
    private bool  sessionStarted  = false;
    private bool  sessionComplete = false;
    private int   completedCycles = 0;

    private enum BreathState { Inhale, HoldIn, Exhale, HoldOut }
    private BreathState currentState = BreathState.Inhale;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    void Start()
    {
        ResetToIdle();
    }

    void Update()
    {
        if (sessionComplete) return;

        if (isHoldingButton)
        {
            RunBreathingStep();
        }
        else if (sessionStarted)
        {
            // Button released mid-session — pause and wait
            instructionText.text = "Hold to continue...";
        }
    }

    // ── Button events (wire to PointerDown / PointerUp / PointerExit) ─────────

    public void OnBreathButtonDown()
    {
        if (sessionComplete) return;
        isHoldingButton = true;
        sessionStarted  = true;
    }

    public void OnBreathButtonUp()
    {
        isHoldingButton = false;
    }

    // ── Core breathing logic ───────────────────────────────────────────────────

    void RunBreathingStep()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case BreathState.Inhale:
                instructionText.text = $"Breathe In... ({completedCycles + 1}/{totalCycles})";
                if (timer >= inhaleDuration) SwitchState(BreathState.HoldIn);
                break;

            case BreathState.HoldIn:
                instructionText.text = $"Hold... ({completedCycles + 1}/{totalCycles})";
                if (timer >= holdInDuration) SwitchState(BreathState.Exhale);
                break;

            case BreathState.Exhale:
                instructionText.text = $"Breathe Out... ({completedCycles + 1}/{totalCycles})";
                if (timer >= exhaleDuration) SwitchState(BreathState.HoldOut);
                break;

            case BreathState.HoldOut:
                instructionText.text = $"Wait... ({completedCycles + 1}/{totalCycles})";
                if (timer >= holdOutDuration) FinishCycle();
                break;
        }
    }

    // ── State management ───────────────────────────────────────────────────────

    // All animator booleans live here — one place, no per-frame spam.
    void SwitchState(BreathState next)
    {
        timer        = 0f;
        currentState = next;
        PlayChime();
        ApplyAnimatorState(next);
    }

    void ApplyAnimatorState(BreathState state)
    {
        // Clear everything first so only one bool is ever true at a time
        animator.SetBool("Idle",     false);
        animator.SetBool("Inhaling", false);
        animator.SetBool("Exhaling", false);

        switch (state)
        {
            case BreathState.Inhale:
                animator.SetBool("Inhaling", true);
                break;
            case BreathState.HoldIn:
                // Keep the inhale pose held at the top
                animator.SetBool("Inhaling", true);
                break;
            case BreathState.Exhale:
                animator.SetBool("Exhaling", true);
                break;
            case BreathState.HoldOut:
                animator.SetBool("Idle", true);
                break;
        }
    }

    void FinishCycle()
    {
        completedCycles++;

        if (completedCycles >= totalCycles)
            TriggerOutro();
        else
            SwitchState(BreathState.Inhale);
    }

    // ── Session end ────────────────────────────────────────────────────────────

    void TriggerOutro()
    {
        sessionComplete      = true;
        isHoldingButton      = false;
        timer                = 0f;
        instructionText.text = "Well done!";

        // FIX: must use StartCoroutine — calling OutroSequence() directly skips all yields
        StartCoroutine(OutroSequence());
    }

    IEnumerator OutroSequence()
    {
        FindFirstObjectByType<ConveyorBelt>()?.SetRunning(false);
        FindFirstObjectByType<ConveyorBelt>()?.ClearAllCards();

        yield return new WaitForSeconds(outroDelay);

        IntroPanel introPanel = FindFirstObjectByType<IntroPanel>(FindObjectsInactive.Include);
        if (introPanel != null)
        {
            introPanel.gameObject.SetActive(true);
            yield return null; // one frame for activation to settle
            introPanel.StartOutro();
        }
        else
        {
            Debug.LogError("SquareBreathing2D: No IntroPanel found in scene!");
        }
    }

    // ── Idle / reset ───────────────────────────────────────────────────────────

    void ResetToIdle()
    {
        timer                = 0f;
        currentState         = BreathState.Inhale;
        instructionText.text = "Hold the button";

        if (animator != null)
        {
            animator.SetBool("Idle",     true);
            animator.SetBool("Inhaling", false);
            animator.SetBool("Exhaling", false);
        }
    }

    public void RestartSession()
    {
        completedCycles = 0;
        sessionComplete = false;
        sessionStarted  = false;
        ResetToIdle();
    }

    // ── Audio ──────────────────────────────────────────────────────────────────

    void PlayChime()
    {
        if (chimeSource != null) chimeSource.Play();
    }
}