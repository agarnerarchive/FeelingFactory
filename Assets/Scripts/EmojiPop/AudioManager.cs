using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip popPositiveClip;
    public AudioClip popNegativeClip;
    public AudioClip levelUpClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    public void PlayPopPositive()  => Play(popPositiveClip);
    public void PlayPopNegative()  => Play(popNegativeClip);
    public void PlayLevelUp()      => Play(levelUpClip);

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}