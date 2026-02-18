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
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.5f;

    private Vector3[] _cornerPositions;
    private Quaternion[] _cornerRotations; 
    private SpriteRenderer[] _jarRenderers;
    private Vector3 _originalCamPos;

    void Start()
    {
        // 1. Determine the size based on spawnPoints to ensure every corner is filled
        int count = spawnPoints.Length;
        _cornerPositions = new Vector3[count];
        _cornerRotations = new Quaternion[count];
        _jarRenderers = new SpriteRenderer[jars.Length];
        _originalCamPos = Camera.main.transform.localPosition;

        for (int i = 0; i < count; i++)
        {
            // Store the FIXED data for this corner slot
            _cornerPositions[i] = spawnPoints[i].position;
            _cornerRotations[i] = spawnPoints[i].rotation; 

            // Initialize covers to match their specific corner's rotation and spawn
            if (i < coverObjects.Length)
            {
                coverObjects[i].transform.position = coverSpawnPoints[i].position;
                coverObjects[i].transform.rotation = _cornerRotations[i];
            }
        }

        for (int i = 0; i < jars.Length; i++)
        {
            _jarRenderers[i] = jars[i].GetComponent<SpriteRenderer>();
        }
        
        StartCoroutine(SlideSwapRoutine());
    }

    IEnumerator SlideSwapRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(swapInterval);

            // 1. ALL COVERS SLIDE TO THEIR ASSIGNED CORNER
            yield return StartCoroutine(MoveAllCovers(true));

            // 2. SHUFFLE & SNAP
            yield return StartCoroutine(ShakeCamera(shakeDuration, shakeIntensity));
            
            ShuffleJars();

            // IMPORTANT: Loop through the Jars and put them in the corresponding Corner Slot
            for (int i = 0; i < jars.Length; i++)
            {
                jars[i].transform.position = _cornerPositions[i];
                jars[i].transform.rotation = _cornerRotations[i];
            }

            // 3. ALL COVERS SLIDE BACK TO THEIR ASSIGNED SPAWN
            yield return StartCoroutine(MoveAllCovers(false));
        }
    }

    IEnumerator MoveAllCovers(bool movingToJar)
    {
        bool allReached = false;
        while (!allReached)
        {
            allReached = true;
            for (int i = 0; i < coverObjects.Length; i++)
            {
                // Cover [0] ALWAYS goes to Corner [0], Cover [1] to Corner [1], etc.
                Vector3 target = movingToJar ? _cornerPositions[i] : coverSpawnPoints[i].position;
                
                coverObjects[i].transform.position = Vector3.MoveTowards(
                    coverObjects[i].transform.position, 
                    target, 
                    slideSpeed * Time.deltaTime
                );

                if (Vector3.Distance(coverObjects[i].transform.position, target) > 0.01f)
                    allReached = false;
            }
            yield return null;
        }
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

    void ShuffleJars()
    {
        for (int i = 0; i < jars.Length; i++)
        {
            int randomIndex = Random.Range(i, jars.Length);
            
            GameObject tempJar = jars[i];
            jars[i] = jars[randomIndex];
            jars[randomIndex] = tempJar;

            SpriteRenderer tempRen = _jarRenderers[i];
            _jarRenderers[i] = _jarRenderers[randomIndex];
            _jarRenderers[randomIndex] = tempRen;
        }
    }
}


