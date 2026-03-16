using UnityEngine;
using TMPro;

public class SquareBreathing2D : MonoBehaviour
{
    [Header("UI Elements")]
    public SpriteRenderer progressRing;
    public SpriteRenderer ballSprite;
    public TMP_Text instructionText;

    [Header("Audio")]
    public AudioSource chimeSource;

    [Header("Outro")]
    public IntroPanel introPanel; // Drag the GameObject that holds IntroPanel here

    [Header("Phase Durations (seconds)")]
    public float inhaleDuration = 4f;
    public float holdInDuration = 4f;
    public float exhaleDuration = 4f;
    public float holdOutDuration = 4f;

    [Header("Settings")]
    public float minScale = 0.25f;
    public float maxScale = 0.3f;
    public int totalCycles = 3;
    public Color inhaleColor = new Color(0.2f, 0.6f, 1f);
    public Color holdColor = new Color(1f, 0.3f, 0.3f);
    public Color completeColor = new Color(0.2f, 0.9f, 0.4f);

    private float timer = 0f;
    private bool isHoldingButton = false;
    private int completedCycles = 0;
    private bool sessionComplete = false;
    private bool sessionStarted = false;

    private enum BreathState { Inhale, HoldIn, Exhale, HoldOut }
    private BreathState currentState = BreathState.Inhale;

    public void OnBreathButtonDown()
    {
        if (sessionComplete) return;
        isHoldingButton = true;
        sessionStarted = true;
    }

    public void OnBreathButtonUp()
    {
        isHoldingButton = false;
    }

    void Update()
    {
        if (sessionComplete) return;

        if (isHoldingButton)
        {
            UpdateBreathing();
        }
        else
        {
            HandleButtonReleased();
        }
    }

    void HandleButtonReleased()
    {
        if (!sessionStarted)
        {
            ResetToIdle();
            return;
        }

        instructionText.text = "Hold to continue...";
    }

    void UpdateBreathing()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case BreathState.Inhale:
            {
                float progress = timer / inhaleDuration;
                instructionText.text = $"Breathe In... ({completedCycles + 1}/{totalCycles})";
                UpdateVisuals(inhaleColor, Mathf.Lerp(minScale, maxScale, progress), progress);
                if (timer >= inhaleDuration) SwitchState(BreathState.HoldIn);
                break;
            }
            case BreathState.HoldIn:
            {
                float progress = timer / holdInDuration;
                instructionText.text = $"Hold... ({completedCycles + 1}/{totalCycles})";
                UpdateVisuals(holdColor, maxScale, progress);
                if (timer >= holdInDuration) SwitchState(BreathState.Exhale);
                break;
            }
            case BreathState.Exhale:
            {
                float progress = timer / exhaleDuration;
                instructionText.text = $"Breathe Out... ({completedCycles + 1}/{totalCycles})";
                UpdateVisuals(inhaleColor, Mathf.Lerp(maxScale, minScale, progress), progress);
                if (timer >= exhaleDuration) SwitchState(BreathState.HoldOut);
                break;
            }
            case BreathState.HoldOut:
            {
                float progress = timer / holdOutDuration;
                instructionText.text = $"Wait... ({completedCycles + 1}/{totalCycles})";
                UpdateVisuals(holdColor, minScale, progress);
                if (timer >= holdOutDuration) CompleteOrContinue();
                break;
            }
        }
    }

    void CompleteOrContinue()
    {
        completedCycles++;

        if (completedCycles >= totalCycles)
        {
            TriggerOutro();
        }
        else
        {
            SwitchState(BreathState.Inhale);
        }
    }

    void TriggerOutro()
    {
        sessionComplete = true;
        isHoldingButton = false;
        timer = 0f;

        instructionText.text = "Well done!";

        transform.localScale = new Vector3(minScale, minScale, 1);
        progressRing.transform.localScale = new Vector3(minScale + 0.8f, minScale + 0.8f, 1);
        progressRing.color = completeColor;

        if (introPanel != null)
        {
            introPanel.StartOutro();
        }
        else
        {
            Debug.LogWarning("SquareBreathing2D: introPanel is not assigned in the Inspector.");
        }
    }

    void UpdateVisuals(Color targetColor, float ballScale, float progress)
    {
        transform.localScale = new Vector3(ballScale, ballScale, 1);
        float ringScale = ballScale + (progress * 0.8f);
        progressRing.transform.localScale = new Vector3(ringScale, ringScale, 1);
        progressRing.color = targetColor;
    }

    void SwitchState(BreathState newState)
    {
        timer = 0;
        currentState = newState;
        if (chimeSource != null) chimeSource.Play();
    }

    void ResetToIdle()
    {
        timer = 0;
        instructionText.text = "Hold the button";
        transform.localScale = new Vector3(minScale, minScale, 1);
        progressRing.transform.localScale = new Vector3(minScale, minScale, 1);
        progressRing.color = Color.gray;
        currentState = BreathState.Inhale;
    }

    public void RestartSession()
    {
        completedCycles = 0;
        sessionComplete = false;
        sessionStarted = false;
        timer = 0f;
        currentState = BreathState.Inhale;
        ResetToIdle();
    }
}