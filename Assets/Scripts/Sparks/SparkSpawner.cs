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
    public GameObject transitionPanel;
    public string nextLevelName;

    [Header("Spawn Locations")]
    public Transform[] spawnPoints;

    [Header("Movement Settings")]
    public float spawnInterval = 2.0f;
    public float warningDelay = 0.5f;
    public float sparkSpeed = 5f;
    public float sparkLifetime = 5f;

    [Header("Difficulty Scaling")]
    [Tooltip("How much the spawn interval decreases per minute elapsed")]
    public float intervalReductionPerMinute = 0.2f;
    [Tooltip("The fastest the spawner is ever allowed to go")]
    public float minimumSpawnInterval = 0.5f;

    private AudioSource _audioSource;
    private List<GameObject> activeSparks = new List<GameObject>();
    private int _lastMinuteChecked = 0;
    private float _currentSpawnInterval;

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        if (transitionPanel != null) transitionPanel.SetActive(false);
    }

    void Start()
    {
        _currentSpawnInterval = spawnInterval;
        StartCoroutine(SpawnLoop());
        launcher = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Remove destroyed sparks from the tracker
        activeSparks.RemoveAll(spark => spark == null);

        // Update the progress meter
        float currentProgress = activeSparks.Count * 0.1f;
        if (progressMeter != null)
            progressMeter.value = currentProgress;

        // Transition when progress reaches 1.0 (10 sparks)
        if (currentProgress >= 1.0f)
        {
            StopAllCoroutines();
            if (transitionPanel != null) transitionPanel.SetActive(true);
        }

        // Check if a new minute has passed and ramp up the speed
        if (GameManager.Instance != null)
        {
            int elapsedIntervals = GameManager.Instance.GetElapsedIntervals();

            if (elapsedIntervals > _lastMinuteChecked)
            {
                _lastMinuteChecked = elapsedIntervals;
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

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }
}



