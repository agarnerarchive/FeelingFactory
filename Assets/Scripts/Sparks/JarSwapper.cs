using UnityEngine;
using System.Collections;

public class JarSwapper : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] jars; 
    public Transform[] spawnPoints; 
    public float slideSpeed = 18f; 
    public float swapInterval = 10f; 
    public float hideDuration = 1.5f;

    private Vector3[] _cornerPositions;
    private Quaternion[] _cornerRotations; // Added to track corner facing
    private SpriteRenderer[] _jarRenderers;

    void Start()
    {
        _cornerPositions = new Vector3[jars.Length];
        _cornerRotations = new Quaternion[jars.Length]; // Initialize rotation array
        _jarRenderers = new SpriteRenderer[jars.Length];

        for (int i = 0; i < jars.Length; i++)
        {
            _cornerPositions[i] = jars[i].transform.position;
            _cornerRotations[i] = jars[i].transform.rotation; // Capture initial rotation
            _jarRenderers[i] = jars[i].GetComponent<SpriteRenderer>();
        }
        
        StartCoroutine(SlideSwapRoutine());
    }

    IEnumerator SlideSwapRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(swapInterval);

            // 1. EXIT
            Vector3[] exitTargets = new Vector3[jars.Length];
            for (int i = 0; i < jars.Length; i++)
            {
                exitTargets[i] = GetClosestSpawnPoint(jars[i].transform.position).position;
            }
            yield return StartCoroutine(MoveAndFade(exitTargets, 0f));

            // 2. SHUFFLE
            ShuffleJars();
            yield return new WaitForSeconds(hideDuration);

            // 3. ENTRY PREP: Teleport and Rotate
            for (int i = 0; i < jars.Length; i++)
            {
                Transform closestSpawn = GetClosestSpawnPoint(_cornerPositions[i]);
                jars[i].transform.position = closestSpawn.position;
                
                // Snap rotation to the correct corner facing
                jars[i].transform.rotation = _cornerRotations[i];
            }

            // 4. RETURN
            yield return StartCoroutine(MoveAndFade(_cornerPositions, 1f));
        }
    }

    IEnumerator MoveAndFade(Vector3[] targets, float targetAlpha)
    {
        bool allReached = false;
        while (!allReached)
        {
            allReached = true;
            for (int i = 0; i < jars.Length; i++)
            {
                jars[i].transform.position = Vector3.MoveTowards(jars[i].transform.position, targets[i], slideSpeed * Time.deltaTime);

                Color c = _jarRenderers[i].color;
                c.a = Mathf.MoveTowards(c.a, targetAlpha, (slideSpeed / 10f) * Time.deltaTime);
                _jarRenderers[i].color = c;

                if (Vector3.Distance(jars[i].transform.position, targets[i]) > 0.05f)
                    allReached = false;
            }
            yield return null;
        }
    }

    // Changed return type to Transform to get more data if needed
    Transform GetClosestSpawnPoint(Vector3 currentPos)
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPoint = spawnPoints[0];
        foreach (Transform sp in spawnPoints)
        {
            float dist = Vector3.Distance(currentPos, sp.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPoint = sp;
            }
        }
        return closestPoint;
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




