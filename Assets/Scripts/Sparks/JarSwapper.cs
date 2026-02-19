using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // Automatically adds AudioSource if missing
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

    [Header("Audio")]
    public AudioClip swapSound; // Drag your sound effect here
    private AudioSource _audioSource;

    private Transform[] _mySpawnPoint;
    private Transform[] _myTargetPoint;
    private Quaternion[] _properCornerRotations; 
    private Quaternion[] _properCoverRotations; 
    private Vector3 _originalCamPos;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (Camera.main != null) _originalCamPos = Camera.main.transform.localPosition;

        _properCornerRotations = new Quaternion[jars.Length];
        _properCoverRotations = new Quaternion[coverObjects.Length];
        _mySpawnPoint = new Transform[coverObjects.Length];
        _myTargetPoint = new Transform[coverObjects.Length];

        // 1. Capture Master Rotations
        for (int i = 0; i < jars.Length; i++)
        {
            _properCornerRotations[i] = jars[i].transform.rotation;
        }

        // 2. Capture Cover Rotations & Lock to Nearest Points
        for (int i = 0; i < coverObjects.Length; i++)
        {
            _properCoverRotations[i] = coverObjects[i].transform.rotation;
            _mySpawnPoint[i] = GetClosestTransform(coverObjects[i].transform.position, coverSpawnPoints);
            _myTargetPoint[i] = GetClosestTransform(coverObjects[i].transform.position, coverTargetPositions);
            
            if (_mySpawnPoint[i] != null)
                coverObjects[i].transform.position = _mySpawnPoint[i].position;

            coverObjects[i].transform.rotation = _properCoverRotations[i];
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

            // 2. AUDIO & SHAKE & SHUFFLE
            if (swapSound != null) _audioSource.PlayOneShot(swapSound);
            
            yield return StartCoroutine(ShakeCamera(shakeDuration, shakeIntensity));
            
            ShuffleJars();

            // 3. TELEPORT
            for (int i = 0; i < jars.Length; i++)
            {
                jars[i].transform.position = spawnPoints[i].position;
                jars[i].transform.rotation = _properCornerRotations[i];
            }

            // 4. SLIDE OUT
            yield return StartCoroutine(MoveAllCovers(false));
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

                coverObjects[i].transform.rotation = _properCoverRotations[i];

                if (Vector3.Distance(coverObjects[i].transform.position, target.position) > 0.05f)
                    allReached = false;
            }
            yield return null;
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








