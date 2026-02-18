using UnityEngine;
using System.Collections;

public class JarSwapper : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] jars; 
    public Transform[] spawnPoints; 
    public float slideSpeed = 18f; 
    public float swapInterval = 10f; 

    [Header("Cover & Shake Settings")]
    public GameObject[] coverObjects;     
    public Transform[] coverSpawnPoints;  
    public Transform[] coverTargetPositions; 
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.5f;

    private Transform[] _mySpawnPoint;
    private Transform[] _myTargetPoint;
    private Vector3 _originalCamPos;

    // We define your specific corner rotations here as constants
    private float[] cornerZRotations = new float[] { -45f, -135f, -135f, 45f };

    void Start()
    {
        if (Camera.main != null) _originalCamPos = Camera.main.transform.localPosition;

        _mySpawnPoint = new Transform[coverObjects.Length];
        _myTargetPoint = new Transform[coverObjects.Length];

        // 1. INITIAL ALIGNMENT
        for (int i = 0; i < jars.Length; i++)
        {
            // Position jar and force its SPECIFIC corner rotation
            jars[i].transform.position = spawnPoints[i].position;
            jars[i].transform.rotation = Quaternion.Euler(0, 0, cornerZRotations[i]);
        }

        // 2. LOCK COVERS
        for (int i = 0; i < coverObjects.Length; i++)
        {
            _mySpawnPoint[i] = GetClosestTransform(coverObjects[i].transform.position, coverSpawnPoints);
            _myTargetPoint[i] = GetClosestTransform(coverObjects[i].transform.position, coverTargetPositions);
            
            if (_mySpawnPoint[i] != null)
                coverObjects[i].transform.position = _mySpawnPoint[i].position;

            // Optional: Make covers match the jar rotation
            coverObjects[i].transform.rotation = Quaternion.Euler(0, 0, cornerZRotations[i]);
        }
        
        StartCoroutine(SlideSwapRoutine());
    }

    IEnumerator SlideSwapRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(swapInterval);

            // 1. SLIDE IN
            yield return StartCoroutine(MoveAllCovers(true));

            // 2. SHAKE & SHUFFLE
            yield return StartCoroutine(ShakeCamera(shakeDuration, shakeIntensity));
            
            // Only swap the GameObjects in the array
            ShuffleJars();

            // 3. TELEPORT & SNAP ROTATION
            for (int i = 0; i < jars.Length; i++)
            {
                // Whichever jar is now at index [i] goes to that corner's position...
                jars[i].transform.position = spawnPoints[i].position;
                
                // ...and gets forced into that corner's FIXED Z-rotation
                jars[i].transform.rotation = Quaternion.Euler(0, 0, cornerZRotations[i]);
            }

            // 4. SLIDE OUT
            yield return StartCoroutine(MoveAllCovers(false));
        }
    }

    void ShuffleJars()
    {
        for (int i = 0; i < jars.Length; i++)
        {
            int randomIndex = Random.Range(i, jars.Length);
            GameObject tempJar = jars[i];
            jars[i] = jars[randomIndex];
            jars[randomIndex] = tempJar;
        }
    }

    IEnumerator MoveAllCovers(bool movingToTarget)
    {
        bool allReached = false;
        while (!allReached)
        {
            allReached = true;
            for (int i = 0; i < coverObjects.Length; i++)
            {
                Transform target = movingToTarget ? _myTargetPoint[i] : _mySpawnPoint[i];
                if (target == null) continue;

                coverObjects[i].transform.position = Vector3.MoveTowards(
                    coverObjects[i].transform.position, 
                    target.position, 
                    slideSpeed * Time.deltaTime
                );

                if (Vector3.Distance(coverObjects[i].transform.position, target.position) > 0.05f)
                    allReached = false;
            }
            yield return null;
        }
    }

    Transform GetClosestTransform(Vector3 currentPos, Transform[] options)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (Transform potentialTarget in options)
        {
            if (potentialTarget == null) continue;
            float dSqrToTarget = (potentialTarget.position - currentPos).sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            Camera.main.transform.localPosition = _originalCamPos + (Vector3)Random.insideUnitCircle * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = _originalCamPos;
    }
}




