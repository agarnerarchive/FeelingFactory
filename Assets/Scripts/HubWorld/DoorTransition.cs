using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Attach this to any GameObject in the scene.
// Hook your button's OnClick to the OnButtonPressed() method.
public class DoorTransition : MonoBehaviour
{
    [Header("Door Animations")]
    [Tooltip("First door AnimationClip")]
    public AnimationClip doorClipOne;
    [Tooltip("GameObject the first clip lives on")]
    public GameObject doorObjectOne;

    [Tooltip("Second door AnimationClip")]
    public AnimationClip doorClipTwo;
    [Tooltip("GameObject the second clip lives on")]
    public GameObject doorObjectTwo;

    [Header("Next Level")]
    public string nextLevelName;

    // Hook this to your button's OnClick in the Inspector
    public void OnButtonPressed()
    {
        StartCoroutine(PlayDoorsAndLoad());
    }

    private IEnumerator PlayDoorsAndLoad()
    {
        float longestClip = 0f;

        // Play both door animations simultaneously
        if (doorClipOne != null && doorObjectOne != null)
        {
            Animation anim = doorObjectOne.GetComponent<Animation>() ?? doorObjectOne.AddComponent<Animation>();
            if (anim.GetClip(doorClipOne.name) == null) anim.AddClip(doorClipOne, doorClipOne.name);
            AnimationState stateOne = anim[doorClipOne.name];
            stateOne.wrapMode = WrapMode.Once;
            stateOne.speed = -1f;
            stateOne.time = doorClipOne.length;
            anim.enabled = true;
            anim.Play(doorClipOne.name);
            longestClip = Mathf.Max(longestClip, doorClipOne.length);
        }

        if (doorClipTwo != null && doorObjectTwo != null)
        {
            Animation anim = doorObjectTwo.GetComponent<Animation>() ?? doorObjectTwo.AddComponent<Animation>();
            if (anim.GetClip(doorClipTwo.name) == null) anim.AddClip(doorClipTwo, doorClipTwo.name);
            AnimationState stateTwo = anim[doorClipTwo.name];
            stateTwo.wrapMode = WrapMode.Once;
            stateTwo.speed = -1f;
            stateTwo.time = doorClipTwo.length;
            anim.enabled = true;
            anim.Play(doorClipTwo.name);
            longestClip = Mathf.Max(longestClip, doorClipTwo.length);
        }

        // Wait for the longer of the two clips to finish
        yield return new WaitForSeconds(longestClip);

        if (string.IsNullOrEmpty(nextLevelName))
        {
            Debug.LogError("DoorTransition: nextLevelName is not set in the Inspector!");
            yield break;
        }

        SceneManager.LoadScene(nextLevelName);
    }
}