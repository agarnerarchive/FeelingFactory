using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SquareBreathing2D : MonoBehaviour
{
    [Header("UI Elements")]
    public SpriteRenderer progressRing;
    public SpriteRenderer ballSprite; // Drag the BreatheBall's SpriteRenderer here
    public TMP_Text instructionText;

    [Header("Audio")]
    public AudioSource chimeSource;

    [Header("Settings")]
    public float minScale = 1f;
    public float maxScale = 4f;
    public Color inhaleColor = new Color(0.2f, 0.6f, 1f); // Blue
    public Color holdColor = new Color(1f, 0.3f, 0.3f);   // Red
    
    private float timer = 0f;
    private const float stepDuration = 4f;

    private enum BreathState { Inhale, HoldIn, Exhale, HoldOut }
    private BreathState currentState = BreathState.Inhale;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            UpdateBreathing();
        }
        else
        {
            ResetToIdle();
        }
    }

    void UpdateBreathing()
{
    timer += Time.deltaTime;
    float progress = timer / stepDuration;

    switch (currentState)
    {
        case BreathState.Inhale:
            instructionText.text = "Breathe In...";
            // Added 'progress' at the end of the brackets
            UpdateVisuals(inhaleColor, Mathf.Lerp(minScale, maxScale, progress), progress); 
            if (timer >= stepDuration) SwitchState(BreathState.HoldIn);
            break;

        case BreathState.HoldIn:
            instructionText.text = "Hold...";
            // Added 'progress' here too
            UpdateVisuals(holdColor, maxScale, progress); 
            if (timer >= stepDuration) SwitchState(BreathState.Exhale);
            break;

        case BreathState.Exhale:
            instructionText.text = "Breathe Out...";
            // Added 'progress' here too
            UpdateVisuals(inhaleColor, Mathf.Lerp(maxScale, minScale, progress), progress); 
            if (timer >= stepDuration) SwitchState(BreathState.HoldOut);
            break;

        case BreathState.HoldOut:
            instructionText.text = "Wait...";
            // Added 'progress' here too
            UpdateVisuals(holdColor, minScale, progress); 
            if (timer >= stepDuration) SwitchState(BreathState.Inhale);
            break;
    }
}


    void UpdateVisuals(Color targetColor, float ballScale, float progress)
{
    // 1. Scale the Ball (The main breathing object)
    transform.localScale = new Vector3(ballScale, ballScale, 1);

    // 2. Scale the Ring (The progress indicator)
    // The ring grows from the ball's size to a bit larger (ballScale + 0.5)
    float ringScale = ballScale + (progress * 0.8f); 
    progressRing.transform.localScale = new Vector3(ringScale, ringScale, 1);

    // 3. Change Colours
    // Only the Ring changes colour to avoid changing the Ball
    // Ensure you change the variable type in the header to SpriteRenderer!
    progressRing.color = targetColor; 
}

    void SwitchState(BreathState newState)
    {
        timer = 0;
        currentState = newState;
        
        // Play the soft chime at every state change
        if (chimeSource != null) chimeSource.Play();
    }

    void ResetToIdle()
{
    timer = 0;
    instructionText.text = "Touch and Hold";
    
    // Reset Scales
    transform.localScale = new Vector3(minScale, minScale, 1);
    progressRing.transform.localScale = new Vector3(minScale, minScale, 1);
    
    // Set Ring to a neutral "waiting" colour
    progressRing.color = Color.gray; 
    currentState = BreathState.Inhale;
}
}

