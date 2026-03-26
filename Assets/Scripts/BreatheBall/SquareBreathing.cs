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
        // Button released — reset back to the beginning of the cycle
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

    // First press — manually apply the initial inhale animation
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
    if (outroStarted) return;
    outroStarted = true;

    Debug.Log("<color=green>SquareBreathing: 3 Cycles Complete. Triggering Outro...</color>");

    // Disable the button immediately so the player can't keep clicking
    if (breathButton != null)
    {
        EventTrigger trigger = breathButton.GetComponent<EventTrigger>();
        if (trigger != null) trigger.enabled = false;
        
        // Optional: Hide the breathing button/text so it doesn't overlap the instructions
        breathButton.gameObject.SetActive(false);
        if (instructionText != null) instructionText.gameObject.SetActive(false);
    }

    StartCoroutine(OutroSequence());
}

IEnumerator OutroSequence()
{
    // Wait the specified delay
    yield return new WaitForSeconds(outroDelay);

    // Find the IntroPanel even if it is currently disabled
    IntroPanel introPanel = FindFirstObjectByType<IntroPanel>(FindObjectsInactive.Include);
    
    if (introPanel != null)
    {
        // 1. Turn the GameObject on
        introPanel.gameObject.SetActive(true);
        
        // 2. Wait exactly one frame so Unity can run "OnEnable" and "Awake"
        yield return null; 
        
        // 3. Fire the StartOutro logic
        introPanel.StartOutro();
    }
    else
    {
        Debug.LogError("SquareBreathing2D: Could not find IntroPanel in this scene! Please ensure the IntroPanel prefab is in the hierarchy.");
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