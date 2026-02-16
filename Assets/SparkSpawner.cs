using UnityEngine;
using System.Collections;

public class SparkSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] sparkPrefabs; // Array of different spark types (Happy, Sad, etc.)
    public float spawnInterval = 2.0f; // Seconds between each spawn
    public Vector2 spawnPadding = new Vector2(1.5f, 1.5f); // Keeps sparks away from screen edges

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnSpark();
        }
    }

    void SpawnSpark()
    {
        // 1. Get random screen position (0.0 to 1.0)
        float randomX = Random.Range(0.1f, 0.9f); 
        float randomY = Random.Range(0.2f, 0.8f);

        // 2. Convert to World Space for the 2D scene
        Vector3 spawnPos = _mainCamera.ViewportToWorldPoint(new Vector3(randomX, randomY, 0));
        spawnPos.z = 0; // Ensure it's on the 2D plane

        // 3. Select a random prefab from your array
        int randomIndex = Random.Range(0, sparkPrefabs.Length);
        
        // 4. Instantiate (Create) the spark
        Instantiate(sparkPrefabs[randomIndex], spawnPos, Quaternion.identity);
    }
}
