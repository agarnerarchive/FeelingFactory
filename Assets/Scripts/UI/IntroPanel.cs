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
    public TextMeshProUGUI buttonText; // Drag the TMP text object directly here

    [Header("Door Animations (open on start, close on outro end)")]
    public AnimationClip introClipOne;
    public GameObject introAnimationObjectOne;

    public AnimationClip introClipTwo;
    public GameObject introAnimationObjectTwo;

    [Header("Looping Animations")]
    public AnimationClip buttonClip;
    public AnimationClip panelImageClip;
    public GameObject panelImageObject;

    [Header("Intro Text Sequence")]
    public string[] introTextSequence;

    [Header("Outro Text Sequence")]
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
    public AudioClip click;

    void Awake()
    {
        // Add Animation components in Awake before any other script's Start runs
        if (buttonClip != null && button != null)
        {
            _buttonAnimation = button.gameObject.GetComponent<Animation>()
                ?? button.gameObject.AddComponent<Animation>();
            if (_buttonAnimation.GetClip(buttonClip.name) == null)
                _buttonAnimation.AddClip(buttonClip, buttonClip.name);
        }

        if (introClipOne != null && introAnimationObjectOne != null)
        {
            _introAnimationOne = introAnimationObjectOne.GetComponent<Animation>()
                ?? introAnimationObjectOne.AddComponent<Animation>();
            if (_introAnimationOne.GetClip(introClipOne.name) == null)
                _introAnimationOne.AddClip(introClipOne, introClipOne.name);
        }

        if (introClipTwo != null && introAnimationObjectTwo != null)
        {
            _introAnimationTwo = introAnimationObjectTwo.GetComponent<Animation>()
                ?? introAnimationObjectTwo.AddComponent<Animation>();
            if (_introAnimationTwo.GetClip(introClipTwo.name) == null)
                _introAnimationTwo.AddClip(introClipTwo, introClipTwo.name);
        }

        if (panelImageClip != null && panelImageObject != null)
        {
            _panelImageAnimation = panelImageObject.GetComponent<Animation>()
                ?? panelImageObject.AddComponent<Animation>();
            if (_panelImageAnimation.GetClip(panelImageClip.name) == null)
                _panelImageAnimation.AddClip(panelImageClip, panelImageClip.name);
        }

        if (buttonText == null)
            Debug.LogWarning("IntroPanel: buttonText is not assigned in the Inspector.");
    }

    void Start()
    {
        introPanel.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.enabled = false;

        SparkSpawner spawner = FindFirstObjectByType<SparkSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
            spawner.enabled = false;

        // Play door open animations ONCE only at level start
        PlayOnce(_introAnimationOne, introClipOne);
        PlayOnce(_introAnimationTwo, introClipTwo);

        PlayLooping(_buttonAnimation, buttonClip);
        PlayLooping(_panelImageAnimation, panelImageClip);

        if (introTextSequence.Length == 0)
        {
            Debug.LogWarning("IntroPanel: introTextSequence is empty.");
            CloseIntroPanel();
            return;
        }

        // Use Update to set the first line — coroutines can be cancelled if the
        // GameObject is disabled between frames, but Update cannot
        _needsInitialText = true;
    }

    void LateUpdate()
{
    if (_needsInitialText)
    {
        _needsInitialText = false;
        string[] sequence = _isOutro ? outroTextSequence : introTextSequence;
        ShowStep(sequence, 0);
    }
}

    // Called by SquareBreathing2D when all cycles complete
    public void StartOutro()
{
    Debug.Log("<color=orange>IntroPanel: StartOutro received call.</color>");
    _isOutro = true;
    _currentIndex = 0;

    // 1. Safe GameManager check
    if (GameManager.Instance != null)
    {
        GameManager.Instance.enabled = false;
    }
    else 
    {
        Debug.LogWarning("IntroPanel: GameManager.Instance not found. Skipping...");
    }

    // 2. Safe Spawner check
    SparkSpawner spawner = FindFirstObjectByType<SparkSpawner>(FindObjectsInactive.Include);
    if (spawner != null)
    {
        spawner.enabled = false;
    }

    // 3. Safe Spark cleanup
    DraggableSpark[] sparks = FindObjectsByType<DraggableSpark>(FindObjectsSortMode.None);
    Debug.Log($"IntroPanel: Found {sparks.Length} sparks to clean up.");
    foreach (DraggableSpark spark in sparks)
    {
        if (spark != null && !spark._isExpiring)
            spark.StartCoroutine(spark.FadeOutAndDestroy());
    }

    // 4. Start the panel sequence
    Debug.Log("IntroPanel: Starting ActivateOutroPanel coroutine.");
    StartCoroutine(ActivateOutroPanel());
}

    private IEnumerator ActivateOutroPanel()
{
    Debug.Log("<color=green>IntroPanel: ActivateOutroPanel coroutine is running!</color>");
    
    // Force the panel to be active and visible
    introPanel.SetActive(true);
    
    // If you have a CanvasGroup, force it to visible
    CanvasGroup cg = introPanel.GetComponent<CanvasGroup>();
    if (cg != null) cg.alpha = 1f;

    yield return null; // Wait for Unity to enable components

    // Manually trigger the first text line immediately instead of waiting for LateUpdate
    if (outroTextSequence != null && outroTextSequence.Length > 0)
    {
        Debug.Log("IntroPanel: Setting initial Outro text: " + outroTextSequence[0]);
        if (buttonText != null) buttonText.text = outroTextSequence[0];
        _currentIndex = 0;
    }
    else
    {
        Debug.LogError("IntroPanel: OUTRO TEXT SEQUENCE IS EMPTY!");
    }

    // Play animations
    PlayLooping(_buttonAnimation, buttonClip);
    PlayLooping(_panelImageAnimation, panelImageClip);
}

    // Hook this to your button's OnClick event in the Inspector
    public void OnButtonPressed()
    {
        string[] sequence = _isOutro ? outroTextSequence : introTextSequence;
        Debug.Log($"IntroPanel: Button pressed. _currentIndex before increment = {_currentIndex}, sequence length = {sequence.Length}, _isOutro = {_isOutro}");
        

        _currentIndex++;
        

        if (_currentIndex < sequence.Length)
        {
            ShowStep(sequence, _currentIndex);
        }
        else
        {
            Debug.Log("IntroPanel: Reached end of sequence.");
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
        Debug.Log($"IntroPanel: ShowStep called. index = {index}, text = '{sequence[index]}'");
        if (buttonText != null)
            buttonText.text = sequence[index];
        else
            Debug.LogWarning("IntroPanel: buttonText is not assigned in the Inspector.");
    }

    private void PlayOnce(Animation anim, AnimationClip clip)
    {
        if (anim == null || clip == null) return;

        if (anim.GetClip(clip.name) == null)
            anim.AddClip(clip, clip.name);

        AnimationState state = anim[clip.name];
        if (state == null) return;

        anim.enabled = true;
        state.speed = 1f;
        state.time = 0f;
        state.wrapMode = WrapMode.Once;
        anim.Play(clip.name);
    }

    private void PlayLooping(Animation anim, AnimationClip clip)
    {
        if (anim == null || clip == null) return;

        if (anim.GetClip(clip.name) == null)
            anim.AddClip(clip, clip.name);

        AnimationState state = anim[clip.name];
        if (state == null)
        {
            Debug.LogWarning("IntroPanel: PlayLooping could not find clip: " + clip.name);
            return;
        }

        anim.enabled = true;
        state.wrapMode = WrapMode.Loop;
        anim.Play(clip.name);
    }

    private void CloseIntroPanel()
    {
        if (_buttonAnimation != null) _buttonAnimation.Stop();
        if (_panelImageAnimation != null) _panelImageAnimation.Stop();

        introPanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.enabled = true;

        SparkSpawner spawner = FindFirstObjectByType<SparkSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
            spawner.enabled = true;
    }

    private IEnumerator PlayReverseAndLoadLevel()
    {
        if (_buttonAnimation != null) _buttonAnimation.Stop();
        if (_panelImageAnimation != null) _panelImageAnimation.Stop();

        float longestClip = 0f;
        if (introClipOne != null) longestClip = Mathf.Max(longestClip, introClipOne.length);
        if (introClipTwo != null) longestClip = Mathf.Max(longestClip, introClipTwo.length);

        // Re-enable and play door close (reverse) animations
        if (_introAnimationOne != null && introClipOne != null)
        {
            _introAnimationOne.enabled = true;
            _introAnimationOne.Stop();

            if (_introAnimationOne.GetClip(introClipOne.name) == null)
                _introAnimationOne.AddClip(introClipOne, introClipOne.name);

            AnimationState stateOne = _introAnimationOne[introClipOne.name];
            if (stateOne != null)
            {
                stateOne.speed = -1f;
                stateOne.time = introClipOne.length;
                _introAnimationOne.Play(introClipOne.name);
            }
            else Debug.LogWarning("IntroPanel: AnimationState for introClipOne is null");
        }

        if (_introAnimationTwo != null && introClipTwo != null)
        {
            _introAnimationTwo.enabled = true;
            _introAnimationTwo.Stop();

            if (_introAnimationTwo.GetClip(introClipTwo.name) == null)
                _introAnimationTwo.AddClip(introClipTwo, introClipTwo.name);

            AnimationState stateTwo = _introAnimationTwo[introClipTwo.name];
            if (stateTwo != null)
            {
                stateTwo.speed = -1f;
                stateTwo.time = introClipTwo.length;
                _introAnimationTwo.Play(introClipTwo.name);
            }
            else Debug.LogWarning("IntroPanel: AnimationState for introClipTwo is null");
        }

        yield return new WaitForSeconds(longestClip);

        if (string.IsNullOrEmpty(nextLevelName))
        {
            Debug.LogError("IntroPanel: nextLevelName is empty! Set it in the Inspector.");
            yield break;
        }

        SceneManager.LoadScene(nextLevelName);
    }
}
