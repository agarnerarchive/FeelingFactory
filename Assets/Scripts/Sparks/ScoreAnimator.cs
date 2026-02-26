using UnityEngine;
using System.Collections;

// Attach this script to any persistent GameObject in your scene (e.g. GameManager).
// Drag your +1 and -1 GameObjects into the Inspector slots.
// Both GameObjects can safely be set to inactive by default.
public class ScoreAnimator : MonoBehaviour
{
    public static ScoreAnimator Instance { get; private set; }

    [Header("Score Animation Objects")]
    [Tooltip("The GameObject that holds your +1 animation")]
    public GameObject gainAnimationObject;

    [Tooltip("The GameObject that holds your -1 animation")]
    public GameObject lossAnimationObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("ScoreAnimator: Instance created on " + gameObject.name);
        Debug.Log("ScoreAnimator: gainAnimationObject = " + (gainAnimationObject != null ? gainAnimationObject.name : "NULL"));
        Debug.Log("ScoreAnimator: lossAnimationObject = " + (lossAnimationObject != null ? lossAnimationObject.name : "NULL"));
    }

    // Called when the player gains a point
    public void PlayGain()
    {
        Debug.Log("ScoreAnimator: PlayGain() called.");

        if (gainAnimationObject != null)
            StartCoroutine(PlayAndDeactivate(gainAnimationObject, "+1"));
        else
            Debug.LogError("ScoreAnimator: gainAnimationObject is not assigned in the Inspector!");
    }

    // Called when the player loses a point
    public void PlayLoss()
    {
        Debug.Log("ScoreAnimator: PlayLoss() called.");

        if (lossAnimationObject != null)
            StartCoroutine(PlayAndDeactivate(lossAnimationObject, "-1"));
        else
            Debug.LogError("ScoreAnimator: lossAnimationObject is not assigned in the Inspector!");
    }

    private IEnumerator PlayAndDeactivate(GameObject obj, string clipName)
    {
        Debug.Log($"ScoreAnimator: Activating {obj.name}");
        obj.SetActive(true);

        Animation anim = obj.GetComponent<Animation>();

        if (anim == null)
        {
            Debug.LogError($"ScoreAnimator: No Animation component found on {obj.name}!");
            yield return new WaitForSeconds(1f);
            obj.SetActive(false);
            yield break;
        }

        Debug.Log($"ScoreAnimator: Animation component found on {obj.name}, enabled = {anim.enabled}");

        AnimationClip clip = anim.GetClip(clipName);

        if (clip == null)
        {
            // Log every clip name on the component to help identify the mismatch
            Debug.LogError($"ScoreAnimator: Could not find clip named '{clipName}' on {obj.name}. Clips available:");
            foreach (AnimationState state in anim)
            {
                Debug.Log($"  - '{state.name}'");
            }
            yield return new WaitForSeconds(1f);
            obj.SetActive(false);
            yield break;
        }

        Debug.Log($"ScoreAnimator: Playing clip '{clipName}' (length: {clip.length}s)");
        anim.enabled = true;
        anim[clipName].wrapMode = WrapMode.Once;
        anim.Play(clipName);

        yield return new WaitForSeconds(0.5f);

        Debug.Log($"ScoreAnimator: Clip finished, deactivating {obj.name}");
        anim.enabled = false;
        obj.SetActive(false);
    }
}