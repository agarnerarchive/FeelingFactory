using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class IntroPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject introPanel;
    public Button button;

    [Header("Intro Animations (play simultaneously on start)")]
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

    private TextMeshProUGUI _buttonText;
    private Animation _introAnimationOne;
    private Animation _introAnimationTwo;
    private Animation _buttonAnimation;
    private Animation _panelImageAnimation;
    private int _currentIndex = 0;
    private bool _isOutro = false;

    void Start()
    {
        _buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (_buttonText == null)
            Debug.LogWarning("IntroPanel: No TextMeshProUGUI found on the button.");

        // Mark all clips as Legacy so they work with the Animation component
        MarkAsLegacy(introClipOne);
        MarkAsLegacy(introClipTwo);
        MarkAsLegacy(buttonClip);
        MarkAsLegacy(panelImageClip);

        if (GameManager.Instance != null)
            GameManager.Instance.enabled = false;

        SparkSpawner spawner = FindFirstObjectByType<SparkSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
            spawner.enabled = false;

        introPanel.SetActive(true);

        // Set up intro clips
        if (introClipOne != null && introAnimationObjectOne != null)
        {
            _introAnimationOne = introAnimationObjectOne.AddComponent<Animation>();
            _introAnimationOne.AddClip(introClipOne, introClipOne.name);
            _introAnimationOne.Play(introClipOne.name);
        }

        if (introClipTwo != null && introAnimationObjectTwo != null)
        {
            _introAnimationTwo = introAnimationObjectTwo.AddComponent<Animation>();
            _introAnimationTwo.AddClip(introClipTwo, introClipTwo.name);
            _introAnimationTwo.Play(introClipTwo.name);
        }

        // Set up looping animations
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

        PlayLooping(_buttonAnimation, buttonClip);
        PlayLooping(_panelImageAnimation, panelImageClip);

        if (introTextSequence.Length == 0)
        {
            Debug.LogWarning("IntroPanel: introTextSequence is empty.");
            CloseIntroPanel();
            return;
        }

        ShowStep(introTextSequence, 0);
    }

    // Called by SparkSpawner when 10 sparks are on screen
    public void StartOutro()
    {
        _isOutro = true;
        _currentIndex = 0;

        if (GameManager.Instance != null)
            GameManager.Instance.enabled = false;

        SparkSpawner spawner = FindFirstObjectByType<SparkSpawner>(FindObjectsInactive.Include);
        if (spawner != null)
            spawner.enabled = false;

        // Explode all sparks still on screen
        foreach (DraggableSpark spark in FindObjectsByType<DraggableSpark>(FindObjectsSortMode.None))
        {
            if (!spark._isExpiring)
                spark.StartCoroutine(spark.FadeOutAndDestroy());
        }

        introPanel.SetActive(true);

        PlayLooping(_buttonAnimation, buttonClip);
        PlayLooping(_panelImageAnimation, panelImageClip);

        if (outroTextSequence.Length == 0)
        {
            Debug.LogWarning("IntroPanel: outroTextSequence is empty.");
            StartCoroutine(PlayReverseAndLoadLevel());
            return;
        }

        ShowStep(outroTextSequence, 0);
    }

    // Hook this to your button's OnClick event in the Inspector
    public void OnButtonPressed()
    {
        _currentIndex++;
        string[] sequence = _isOutro ? outroTextSequence : introTextSequence;

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
        if (_buttonText != null)
            _buttonText.text = sequence[index];
    }

    // Safely plays a clip looping — re-adds the clip if it was lost when the panel was inactive
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
        if (_introAnimationOne != null) _introAnimationOne.Stop();
        if (_introAnimationTwo != null) _introAnimationTwo.Stop();
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

        // Calculate wait time directly from clip assets — never 0 even if AnimationState fails
        float longestClip = 0f;
        if (introClipOne != null) longestClip = Mathf.Max(longestClip, introClipOne.length);
        if (introClipTwo != null) longestClip = Mathf.Max(longestClip, introClipTwo.length);

        Debug.Log($"IntroPanel: Reverse animation duration = {longestClip}s, loading '{nextLevelName}'");

        if (_introAnimationOne != null && introClipOne != null)
        {
            _introAnimationOne.Stop();
            if (_introAnimationOne.GetClip(introClipOne.name) == null)
                _introAnimationOne.AddClip(introClipOne, introClipOne.name);

            AnimationState stateOne = _introAnimationOne[introClipOne.name];
            if (stateOne != null)
            {
                stateOne.speed = -1f;
                stateOne.time = introClipOne.length;
                _introAnimationOne.enabled = true;
                _introAnimationOne.Play(introClipOne.name);
                Debug.Log("IntroPanel: introClipOne playing in reverse");
            }
            else Debug.LogWarning("IntroPanel: AnimationState for introClipOne is null");
        }

        if (_introAnimationTwo != null && introClipTwo != null)
        {
            _introAnimationTwo.Stop();
            if (_introAnimationTwo.GetClip(introClipTwo.name) == null)
                _introAnimationTwo.AddClip(introClipTwo, introClipTwo.name);

            AnimationState stateTwo = _introAnimationTwo[introClipTwo.name];
            if (stateTwo != null)
            {
                stateTwo.speed = -1f;
                stateTwo.time = introClipTwo.length;
                _introAnimationTwo.enabled = true;
                _introAnimationTwo.Play(introClipTwo.name);
                Debug.Log("IntroPanel: introClipTwo playing in reverse");
            }
            else Debug.LogWarning("IntroPanel: AnimationState for introClipTwo is null");
        }

        // Always wait the full clip length regardless of whether states were found
        yield return new WaitForSeconds(longestClip);

        if (string.IsNullOrEmpty(nextLevelName))
        {
            Debug.LogError("IntroPanel: nextLevelName is empty! Set it in the Inspector.");
            yield break;
        }

        SceneManager.LoadScene(nextLevelName);
    }
    private void MarkAsLegacy(AnimationClip clip)
    {
        if (clip == null) return;
#if UNITY_EDITOR
        SerializedObject so = new SerializedObject(clip);
        so.FindProperty("m_Legacy").boolValue = true;
        so.ApplyModifiedProperties();
#endif
    }
}