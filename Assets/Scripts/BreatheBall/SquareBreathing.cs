using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class SquareBreathing2D : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text instructionText;

    // Drag the button GameObject here so we can disable its EventTrigger on completion
    public Button breathButton;

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
    private bool  outroStarted    = false;
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
            // Button released — reset cycle back to the beginning
            sessionStarted  = false;
            isHoldingButton = false;
            completedCycles = 0;
            ResetToIdle();
        }
    }

    // ── Button events ──────────────────────────────────────────────────────────
    // Wire via Event Trigger: PointerDown / PointerUp / PointerExit

    public void OnBreathButtonDown()
    {
        if (sessionComplete) return;
        isHoldingButton = true;

        // First press — manually kick off the inhale animation
        if (!sessionStarted)
        {
            sessionStarted = true;
            ApplyAnimatorState(BreathState.Inhale);
        }
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

    void SwitchState(BreathState next)
    {
        timer        = 0f;
        currentState = next;
        PlayChime();
        ApplyAnimatorState(next);
    }

    void ApplyAnimatorState(BreathState state)
    {
        // Clear all bools first so only one is ever true at a time
        animator.SetBool("Idle",     false);
        animator.SetBool("Inhaling", false);
        animator.SetBool("Exhaling", false);

        switch (state)
        {
            case BreathState.Inhale:
            case BreathState.HoldIn:
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
        // Guard against iOS stray touch events firing this twice
        if (outroStarted) return;
        outroStarted = true;

        sessionComplete      = true;
        isHoldingButton      = false;
        timer                = 0f;
        instructionText.text = "Well done!";

        // Disable the EventTrigger so no further touches can get through
        if (breathButton != null)
        {
            EventTrigger trigger = breathButton.GetComponent<EventTrigger>();
            if (trigger != null) trigger.enabled = false;
        }

        StartCoroutine(OutroSequence());
    }

    IEnumerator OutroSequence()
    {
        yield return new WaitForSeconds(outroDelay);

        IntroPanel introPanel = FindFirstObjectByType<IntroPanel>(FindObjectsInactive.Include);
        if (introPanel != null)
        {
            // Set the outro flag BEFORE activating so IntroPanel.Start() skips the door open animations
            introPanel.PrepareForOutro();
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
        outroStarted    = false;

        // Re-enable the EventTrigger so the button works again
        if (breathButton != null)
        {
            EventTrigger trigger = breathButton.GetComponent<EventTrigger>();
            if (trigger != null) trigger.enabled = true;
        }

        ResetToIdle();
    }

    // ── Audio ──────────────────────────────────────────────────────────────────

    void PlayChime()
    {
        if (chimeSource != null) chimeSource.Play();
    }
}
