using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class IntroPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject introPanel;
    public Button button;
    public TextMeshProUGUI buttonText;

    [Header("Door Animations")]
    public AnimationClip introClipOne;
    public GameObject introAnimationObjectOne;
    public AnimationClip introClipTwo;
    public GameObject introAnimationObjectTwo;

    [Header("Looping Animations")]
    public AnimationClip buttonClip;
    public AnimationClip panelImageClip;
    public GameObject panelImageObject;

    [Header("Text Sequences")]
    public string[] introTextSequence;
    public string[] outroTextSequence;

    [Header("Next Level")]
    public string nextLevelName;

    private Animation _introAnimationOne;
    private Animation _introAnimationTwo;
    private Animation _buttonAnimation;
    private Animation _panelImageAnimation;
    private int _currentIndex = 0;
    private bool _isOutro = false;
    private bool _needsInitialText = false;
    private bool _isInitialized = false;

    // 1. We use Awake to make sure components are ready before Start()
    void Awake()
    {
        InitializeAnimations();
    }

    public void InitializeAnimations()
    {
        if (_isInitialized) return;

        _buttonAnimation = SetupAnim(button?.gameObject, buttonClip);
        _introAnimationOne = SetupAnim(introAnimationObjectOne, introClipOne);
        _introAnimationTwo = SetupAnim(introAnimationObjectTwo, introClipTwo);
        _panelImageAnimation = SetupAnim(panelImageObject, panelImageClip);

        _isInitialized = true;
    }

    private Animation SetupAnim(GameObject obj, AnimationClip clip)
    {
        if (obj == null || clip == null) return null;
        Animation anim = obj.GetComponent<Animation>() ?? obj.AddComponent<Animation>();
        
        // This adds the clip to the library so the script can find it by name
        if (anim.GetClip(clip.name) == null) 
        {
            anim.AddClip(clip, clip.name);
        }
        
        anim.playAutomatically = false;
        return anim;
    }

    void Start()
    {
        // On level start, if we aren't in 'Outro' mode, play the Opening
        if (!_isOutro)
        {
            introPanel.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.enabled = false;
            
            // Explicitly play the Open animation (Speed 1, Time 0)
            PlayOnce(_introAnimationOne, introClipOne, 1f, 0f);
            PlayOnce(_introAnimationTwo, introClipTwo, 1f, 0f);

            PlayLooping(_buttonAnimation, buttonClip);
            PlayLooping(_panelImageAnimation, panelImageClip);
            _needsInitialText = true;
        }
    }

    void LateUpdate()
    {
        if (_needsInitialText)
        {
            _needsInitialText = false;
            string[] sequence = _isOutro ? outroTextSequence : introTextSequence;
            if (sequence.Length > 0) ShowStep(sequence, 0);
        }
    }

    public void StartOutro()
    {
        _isOutro = true;
        _currentIndex = 0;
        
        // Ensure initialized if the script was disabled at scene start
        InitializeAnimations();

        if (GameManager.Instance != null) GameManager.Instance.enabled = false;

        StartCoroutine(ActivateOutroPanel());
    }

    private IEnumerator ActivateOutroPanel()
    {
        // 2. Snap doors to the "Open" frame and hide them
        PrepareDoorForInstructions(_introAnimationOne, introClipOne, introAnimationObjectOne);
        PrepareDoorForInstructions(_introAnimationTwo, introClipTwo, introAnimationObjectTwo);

        introPanel.SetActive(true);
        yield return null; 

        if (outroTextSequence != null && outroTextSequence.Length > 0)
        {
            if (buttonText != null) buttonText.text = outroTextSequence[0];
        }
        else
        {
            StartCoroutine(PlayReverseAndLoadLevel());
            yield break;
        }

        PlayLooping(_buttonAnimation, buttonClip);
        PlayLooping(_panelImageAnimation, panelImageClip);
    }

    private void PrepareDoorForInstructions(Animation anim, AnimationClip clip, GameObject obj)
    {
        if (anim != null && clip != null)
        {
            anim.enabled = true;
            AnimationState state = anim[clip.name];
            if (state != null)
            {
                state.time = clip.length; // Set to the end of the clip (fully open)
                state.weight = 1;
                anim.Sample(); // Apply the state
            }
            anim.Stop(); // Don't let it keep playing
        }
        
        if (obj != null) obj.SetActive(false); // Hide the actual door
    }

    public void OnButtonPressed()
    {
        string[] sequence = _isOutro ? outroTextSequence : introTextSequence;
        _currentIndex++;

        if (_currentIndex < sequence.Length)
        {
            ShowStep(sequence, _currentIndex);
        }
        else
        {
            if (_isOutro)
            {
                button.interactable = false;
                StartCoroutine(PlayReverseAndLoadLevel());
            }
            else
            {
                CloseIntroPanel();
            }
        }
    }

    private void ShowStep(string[] sequence, int index)
    {
        if (buttonText != null) buttonText.text = sequence[index];
    }

    private void PlayOnce(Animation anim, AnimationClip clip, float speed, float startTime)
    {
        if (anim == null || clip == null) return;
        anim.enabled = true;
        
        AnimationState state = anim[clip.name];
        if (state != null)
        {
            state.speed = speed;
            state.time = startTime;
            state.wrapMode = WrapMode.Once;
            anim.Play(clip.name);
        }
    }

    private void PlayLooping(Animation anim, AnimationClip clip)
    {
        if (anim == null || clip == null) return;
        anim.enabled = true;
        AnimationState state = anim[clip.name];
        if (state != null)
        {
            state.wrapMode = WrapMode.Loop;
            anim.Play(clip.name);
        }
    }

    private void CloseIntroPanel()
    {
        if (_buttonAnimation != null) _buttonAnimation.Stop();
        if (_panelImageAnimation != null) _panelImageAnimation.Stop();
        introPanel.SetActive(false);

        if (GameManager.Instance != null) GameManager.Instance.enabled = true;
        SparkSpawner spawner = FindFirstObjectByType<SparkSpawner>(FindObjectsInactive.Include);
        if (spawner != null) spawner.enabled = true;
    }

    private IEnumerator PlayReverseAndLoadLevel()
    {
        if (_buttonAnimation != null) _buttonAnimation.Stop();
        if (_panelImageAnimation != null) _panelImageAnimation.Stop();

        // 3. Show doors again for the final closing
        if (introAnimationObjectOne != null) introAnimationObjectOne.SetActive(true);
        if (introAnimationObjectTwo != null) introAnimationObjectTwo.SetActive(true);

        float longestClip = 0f;
        if (introClipOne != null) longestClip = Mathf.Max(longestClip, introClipOne.length);
        if (introClipTwo != null) longestClip = Mathf.Max(longestClip, introClipTwo.length);

        // Play Reverse (Speed -1, start at end of clip)
        PlayOnce(_introAnimationOne, introClipOne, -1f, introClipOne ? introClipOne.length : 0);
        PlayOnce(_introAnimationTwo, introClipTwo, -1f, introClipTwo ? introClipTwo.length : 0);

        yield return new WaitForSeconds(longestClip);

        if (!string.IsNullOrEmpty(nextLevelName))
            SceneManager.LoadScene(nextLevelName);
    }
}
