using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SparkSpawner : MonoBehaviour
{
    [Header("Assets")]
    public GameObject[] sparkPrefabs;
    public GameObject spawnParticlePrefab;
    public AudioClip spawnSound;
    public AudioSource launcher;

    [Header("UI Elements")]
    public Slider progressMeter;

    [Header("Spawn Locations")]
    public Transform[] spawnPoints;

    [Header("Movement Settings")]
    public float spawnInterval = 2.0f;
    public float warningDelay = 0.5f;
    public float sparkSpeed = 5f;
    public float sparkLifetime = 5f;

    [Header("Warning (8+ Sparks)")]
    [Tooltip("GameObject the warning animation lives on")]
    public GameObject warningAnimationObject;
    [Tooltip("Warning AnimationClip to loop when 8+ sparks are on screen")]
    public AnimationClip warningClip;
    [Tooltip("Sound to loop when 8+ sparks are on screen")]
    public AudioClip warningSound;

    private Animation _warningAnimation;
    private AudioSource _warningAudio;
    private bool _warningActive = false;

    [Header("Difficulty Scaling")]
    [Tooltip("How much the spawn interval decreases per 30 seconds elapsed")]
    public float intervalReductionPerMinute = 0.2f;
    [Tooltip("The fastest the spawner is ever allowed to go")]
    public float minimumSpawnInterval = 0.5f;

    private AudioSource _audioSource;
    private List<GameObject> activeSparks = new List<GameObject>();
    private int _lastIntervalChecked = 0;
    private float _currentSpawnInterval;
    private bool _outroTriggered = false;

    [Header("Outro Settings")]
    [Tooltip("Delay in seconds before the outro panel appears")]
    public float outroDelay = 1.5f;

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    void Start()
    {
        StartCoroutine (StartDelay());

        _currentSpawnInterval = spawnInterval;
        //StartCoroutine(SpawnLoop());
        launcher = GetComponent<AudioSource>();

        // Set up warning animation
        if (warningClip != null && warningAnimationObject != null)
        {
            _warningAnimation = warningAnimationObject.GetComponent<Animation>() ?? warningAnimationObject.AddComponent<Animation>();
            if (_warningAnimation.GetClip(warningClip.name) == null) _warningAnimation.AddClip(warningClip, warningClip.name);
            _warningAnimation[warningClip.name].wrapMode = WrapMode.Loop;
        }

        // Set up warning audio on a separate AudioSource
        if (warningSound != null)
        {
            _warningAudio = gameObject.AddComponent<AudioSource>();
            _warningAudio.clip = warningSound;
            _warningAudio.loop = true;
            _warningAudio.playOnAwake = false;
        }
    }

    void Update()
    {
        // Remove destroyed sparks from the tracker
        activeSparks.RemoveAll(spark => spark == null);

        // Update the progress meter
        float currentProgress = activeSparks.Count * 0.1f;
        if (progressMeter != null)
            progressMeter.value = currentProgress;

        // Play warning animation and sound when 8 or more sparks are on screen
        if (activeSparks.Count >= 8 && !_warningActive && !_outroTriggered)
        {
            _warningActive = true;
            if (_warningAnimation != null) { _warningAnimation.enabled = true; _warningAnimation.Play(warningClip.name); }
            if (_warningAudio != null) _warningAudio.Play();
        }
        else if (activeSparks.Count < 8 && _warningActive)
        {
            _warningActive = false;
            if (_warningAnimation != null) { _warningAnimation.Stop(); _warningAnimation.enabled = false; }
            if (_warningAudio != null) _warningAudio.Stop();
        }

        // Trigger the outro when 10 sparks are on screen
        if (currentProgress >= 1.0f && !_outroTriggered)
        {
            _outroTriggered = true;
            StopAllCoroutines();

            // Stop warning animation and sound when game ends
            _warningActive = false;
            if (_warningAnimation != null) { _warningAnimation.Stop(); _warningAnimation.enabled = false; }
            if (_warningAudio != null) _warningAudio.Stop();

            StartCoroutine(OutroDelayRoutine());
        }

        // Check if a new 30-second interval has passed and ramp up speed
        if (GameManager.Instance != null)
        {
            int elapsedIntervals = GameManager.Instance.GetElapsedIntervals();

            if (elapsedIntervals > _lastIntervalChecked)
            {
                _lastIntervalChecked = elapsedIntervals;
                _currentSpawnInterval = Mathf.Max(
                    minimumSpawnInterval,
                    spawnInterval - (intervalReductionPerMinute * elapsedIntervals)
                );
                Debug.Log($"SparkSpawner: 30s interval {elapsedIntervals} reached. New spawn interval: {_currentSpawnInterval}s");
            }
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_currentSpawnInterval);
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            StartCoroutine(WarningSequence(point));
            if (launcher != null) launcher.Play();
        }
    }

    IEnumerator WarningSequence(Transform point)
    {
        if (spawnParticlePrefab != null)
        {
            GameObject effect = Instantiate(spawnParticlePrefab, point.position, point.rotation);
            Destroy(effect, 1.0f);
        }

        yield return new WaitForSeconds(warningDelay);

        if (spawnSound != null) _audioSource.PlayOneShot(spawnSound, 0.6f);

        GameObject sparkObj = Instantiate(sparkPrefabs[Random.Range(0, sparkPrefabs.Length)], point.position, point.rotation);

        activeSparks.Add(sparkObj);

        Rigidbody2D rb = sparkObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = point.up * sparkSpeed;
    }

    private IEnumerator OutroDelayRoutine()
    {
        yield return new WaitForSeconds(outroDelay);

        IntroPanel introPanel = FindFirstObjectByType<IntroPanel>(FindObjectsInactive.Include);
    if (introPanel != null)
    {
        // Activate the GameObject first, then call StartOutro
        introPanel.gameObject.SetActive(true);
        yield return null; // wait one frame for activation to complete
        introPanel.StartOutro();
    }
    else
    {
        Debug.LogError("EmojiCharacter: No IntroPanel found in scene!");
    }
}
    public void LoadNextLevel()
    {
        SceneManager.LoadScene("nextLevelName");
    }

     IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(10.0f);
        StartCoroutine(SpawnLoop());
    }
}