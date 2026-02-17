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
    private SpriteRenderer[] _jarRenderers; // To control transparency

    void Start()
    {
        _cornerPositions = new Vector3[jars.Length];
        _jarRenderers = new SpriteRenderer[jars.Length];

        for (int i = 0; i < jars.Length; i++)
        {
            _cornerPositions[i] = jars[i].transform.position;
            _jarRenderers[i] = jars[i].GetComponent<SpriteRenderer>();
        }
        
        StartCoroutine(SlideSwapRoutine());
    }

    IEnumerator SlideSwapRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(swapInterval);

            // 1. EXIT: Find closest point and fade OUT
            Vector3[] exitTargets = new Vector3[jars.Length];
            for (int i = 0; i < jars.Length; i++)
            {
                exitTargets[i] = GetClosestSpawnPoint(jars[i].transform.position);
            }
            yield return StartCoroutine(MoveAndFade(exitTargets, 0f)); // Fade to 0 (Transparent)

            // 2. SHUFFLE & HIDE
            ShuffleJars();
            yield return new WaitForSeconds(hideDuration);

            // 3. ENTRY PREP: Teleport to new closest spawn point
            for (int i = 0; i < jars.Length; i++)
            {
                jars[i].transform.position = GetClosestSpawnPoint(_cornerPositions[i]);
            }

            // 4. RETURN: Slide back and fade IN
            yield return StartCoroutine(MoveAndFade(_cornerPositions, 1f)); // Fade to 1 (Opaque)
        }
    }

    // Unified Coroutine to handle movement AND transparency
    IEnumerator MoveAndFade(Vector3[] targets, float targetAlpha)
    {
        bool allReached = false;
        while (!allReached)
        {
            allReached = true;
            for (int i = 0; i < jars.Length; i++)
            {
                // Move Position
                jars[i].transform.position = Vector3.MoveTowards(jars[i].transform.position, targets[i], slideSpeed * Time.deltaTime);

                // Update Transparency (Lerp Alpha)
                Color c = _jarRenderers[i].color;
                // We move the alpha value toward the targetAlpha (0 or 1)
                c.a = Mathf.MoveTowards(c.a, targetAlpha, (slideSpeed / 10f) * Time.deltaTime);
                _jarRenderers[i].color = c;

                if (Vector3.Distance(jars[i].transform.position, targets[i]) > 0.05f)
                    allReached = false;
            }
            yield return null;
        }
    }

    Vector3 GetClosestSpawnPoint(Vector3 currentPos)
    {
        float closestDistance = Mathf.Infinity;
        Vector3 closestPoint = spawnPoints[0].position;
        foreach (Transform sp in spawnPoints)
        {
            float dist = Vector3.Distance(currentPos, sp.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPoint = sp.position;
            }
        }
        return closestPoint;
    }

    void ShuffleJars()
    {
        // When we shuffle the GameObjects, we must also shuffle the Renderers 
        // so the transparency control stays with the right jar
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




