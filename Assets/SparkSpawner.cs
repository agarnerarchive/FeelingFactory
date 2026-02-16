using UnityEngine;
using System.Collections;

public class SparkSpawner : MonoBehaviour
{
    [Header("Assets")]
    public GameObject[] sparkPrefabs;
    public GameObject spawnParticlePrefab; 
    public AudioClip spawnSound;           
    
    [Header("Spawn Locations")]
    public Transform[] spawnPoints;
    public Transform factoryCenter; // Create an empty object in the middle of the screen!

    [Header("Movement Settings")]
    public float spawnInterval = 2.0f;
    public float warningDelay = 0.5f; 
    public float sparkSpeed = 5f; // Speed of the straight line move

    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
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
        // 1. Small Warning Particle
        if (spawnParticlePrefab != null)
        {
            GameObject effect = Instantiate(spawnParticlePrefab, point.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * 0.3f;
            Destroy(effect, 1f);
        }

        yield return new WaitForSeconds(warningDelay);

        // 2. Play Sound
        if (spawnSound != null) _audioSource.PlayOneShot(spawnSound, 0.6f);
        
        // 3. Spawn & Launch in a Straight Line
        GameObject sparkObj = Instantiate(sparkPrefabs[Random.Range(0, sparkPrefabs.Length)], point.position, Quaternion.identity);
        Rigidbody2D rb = sparkObj.GetComponent<Rigidbody2D>();

        if (rb != null && factoryCenter != null)
        {
            // Calculate direction from spawn point to the center of the screen
            Vector2 direction = (factoryCenter.position - point.position).normalized;
            rb.linearVelocity = direction * sparkSpeed; // Moves in a perfect straight line
        }
    }
}



