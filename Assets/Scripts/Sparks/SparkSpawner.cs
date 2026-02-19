using UnityEngine;
using UnityEngine.UI; // For Slider
using UnityEngine.SceneManagement; // For changing levels
using System.Collections;
using System.Collections.Generic; // For Lists

public class SparkSpawner : MonoBehaviour
{
    [Header("Assets")]
    public GameObject[] sparkPrefabs;
    public GameObject spawnParticlePrefab; 
    public AudioClip spawnSound;           
    
    [Header("UI Elements")]
    public Slider progressMeter;
    public GameObject transitionPanel;
    public string nextLevelName; // Name of the scene to load

    [Header("Spawn Locations")]
    public Transform[] spawnPoints;

    [Header("Movement Settings")]
    public float spawnInterval = 2.0f;
    public float warningDelay = 0.5f; 
    public float sparkSpeed = 5f;
    public float sparkLifetime = 5f; // How long they stay on screen

    private AudioSource _audioSource;
    private List<GameObject> activeSparks = new List<GameObject>(); // Track sparks

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        if(transitionPanel != null) transitionPanel.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void Update()
{
    // 1. Remove references to any sparks that have been destroyed
    activeSparks.RemoveAll(spark => spark == null);
    
    // 2. Calculate the progress (0.1 per spark)
    float currentProgress = activeSparks.Count * 0.1f;

    // 3. Update the UI meter
    if (progressMeter != null)
    {
        progressMeter.value = currentProgress;
    }

    // 4. Transition when progress reaches 1.0 (10 sparks)
    if (currentProgress >= 1.0f)
    {
        StopAllCoroutines(); 
        if (transitionPanel != null) transitionPanel.SetActive(true);
    }
}


    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            StartCoroutine(WarningSequence(point));
        }
    }

    IEnumerator WarningSequence(Transform point)
    {
        if (spawnParticlePrefab != null)
        {
            GameObject effect = Instantiate(spawnParticlePrefab, point.position, point.rotation);
            Destroy(effect, 1f);
        }

        yield return new WaitForSeconds(warningDelay);

        if (spawnSound != null) _audioSource.PlayOneShot(spawnSound, 0.6f);
        
        GameObject sparkObj = Instantiate(sparkPrefabs[Random.Range(0, sparkPrefabs.Length)], point.position, point.rotation);
        
        // Add to our tracker
        activeSparks.Add(sparkObj);

        Rigidbody2D rb = sparkObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = point.up * sparkSpeed; 
    }

    // Call this from your UI Button's OnClick event
    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }
}



