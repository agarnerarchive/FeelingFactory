using UnityEngine;
using UnityEngine.Audio;
using System.Collections;


public class delay_audio : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip clip;

    IEnumerator PlaySoundAfterDelay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
    }

    void Start()
    {
        StartCoroutine(PlaySoundAfterDelay(audioSource, 0.1f));
    }

}
