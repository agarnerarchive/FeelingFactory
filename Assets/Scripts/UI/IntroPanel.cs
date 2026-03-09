using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach this to your intro panel GameObject.
// Hook your button's OnClick event to the OnButtonPressed() method.
public class IntroPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject introPanel;
    public Button button;

    [Header("Animations")]
    [Tooltip("Drag your button AnimationClip asset in here")]
    public AnimationClip buttonClip;

    [Tooltip("Drag your panel image AnimationClip asset in here")]
    public AnimationClip panelImageClip;
    public GameObject panelImageObject;

    [Header("Text Sequence")]
    [Tooltip("Each entry is one step — the button text changes with each press")]
    public string[] textSequence;

    private TextMeshProUGUI _buttonText;
    private Animation _buttonAnimation;
    private Animation _panelImageAnimation;
    private int _currentIndex = 0;

    void Start()
    {
        _buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (_buttonText == null)
            Debug.LogWarning("IntroPanel: No TextMeshProUGUI found on the button.");

        // Pause the game by disabling GameManager and SparkSpawner
        // This leaves Time.timeScale alone so animations play freely
        if (GameManager.Instance != null)
            GameManager.Instance.enabled = false;

        SparkSpawner spawner = FindObjectOfType<SparkSpawner>();
        if (spawner != null)
            spawner.enabled = false;

        introPanel.SetActive(true);

        // Add Animation components and assign clips
        if (buttonClip != null)
        {
            _buttonAnimation = button.gameObject.AddComponent<Animation>();
            _buttonAnimation.AddClip(buttonClip, buttonClip.name);
        }

        if (panelImageClip != null && panelImageObject != null)
        {
            _panelImageAnimation = panelImageObject.AddComponent<Animation>();
            _panelImageAnimation.AddClip(panelImageClip, panelImageClip.name);
        }

        // Start both looping immediately
        PlayLooping(_buttonAnimation, buttonClip);
        PlayLooping(_panelImageAnimation, panelImageClip);

        if (textSequence.Length == 0)
        {
            Debug.LogWarning("IntroPanel: textSequence is empty — closing panel immediately.");
            ClosePanel();
            return;
        }

        ShowStep(0);
    }

    // Hook this to your button's OnClick event in the Inspector
    public void OnButtonPressed()
    {
        _currentIndex++;

        if (_currentIndex < textSequence.Length)
            ShowStep(_currentIndex);
        else
            ClosePanel();
    }

    private void ShowStep(int index)
    {
        if (_buttonText != null)
            _buttonText.text = textSequence[index];
    }

    private void PlayLooping(Animation anim, AnimationClip clip)
    {
        if (anim == null || clip == null) return;

        anim.enabled = true;
        anim[clip.name].wrapMode = WrapMode.Loop;
        anim.Play(clip.name);
    }

    private void ClosePanel()
    {
        if (_buttonAnimation != null) _buttonAnimation.Stop();
        if (_panelImageAnimation != null) _panelImageAnimation.Stop();

        introPanel.SetActive(false);

        // Re-enable the game
        if (GameManager.Instance != null)
            GameManager.Instance.enabled = true;

        SparkSpawner spawner = FindObjectOfType<SparkSpawner>();
        if (spawner != null)
            spawner.enabled = true;
    }
}