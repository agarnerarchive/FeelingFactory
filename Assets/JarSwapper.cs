using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JarSwapper : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] jars; 
    public GameObject poofEffectPrefab; 
    public float swapInterval = 10f; 
    public float warningDuration = 2f; 

    private Vector3[] _cornerPositions;

    void Start()
    {
        _cornerPositions = new Vector3[jars.Length];
        for (int i = 0; i < jars.Length; i++)
        {
            _cornerPositions[i] = jars[i].transform.position;
        }
        StartCoroutine(SwapRoutine());
    }

    IEnumerator SwapRoutine()
{
    while (true)
    {
        // 1. Wait until it's time to swap
        yield return new WaitForSeconds(swapInterval - warningDuration);

        // 2. QUICK WARNING: Short shake (e.g. 1 second)
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            for (int i = 0; i < jars.Length; i++)
            {
                // Very fast jitter
                jars[i].transform.position = _cornerPositions[i] + (Vector3)Random.insideUnitCircle * 0.15f;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. INSTANT VANISH: Play effect and hide
        foreach (GameObject jar in jars)
        {
            if (poofEffectPrefab != null) {
                GameObject poof = Instantiate(poofEffectPrefab, jar.transform.position, Quaternion.identity);
                Destroy(poof, 0.5f); // Ensure the particle cleans up fast
            }
            jar.SetActive(false); 
        }

        // REDUCED PAUSE: Only 0.2 seconds of "blind" time
        yield return new WaitForSeconds(0.2f); 

        // 4. QUICK RESPAWN: Move and show
        ShufflePositions();
        
        for (int i = 0; i < jars.Length; i++)
        {
            jars[i].SetActive(true);
            _cornerPositions[i] = jars[i].transform.position; // Lock new home
            
            if (poofEffectPrefab != null) {
                GameObject poof = Instantiate(poofEffectPrefab, jars[i].transform.position, Quaternion.identity);
                Destroy(poof, 0.5f);
            }
        }
    }
}


    void ShufflePositions()
    {
        // Shuffle the GameObjects themselves in the array to mix them up
        for (int i = 0; i < jars.Length; i++)
        {
            GameObject temp = jars[i];
            int randomIndex = Random.Range(i, jars.Length);
            jars[i] = jars[randomIndex];
            jars[randomIndex] = temp;
        }

        // Now place the shuffled jars back into the fixed 4 corner positions
        // We use a separate array of vectors so the corners never change, only the jars do
        Vector3[] fixedCorners = new Vector3[] {
            new Vector3(-7, 4, 0),  // Top Left
            new Vector3(7, 4, 0),   // Top Right
            new Vector3(-7, -4, 0), // Bottom Left
            new Vector3(7, -4, 0)   // Bottom Right
        };

        // If you want to use the positions you set in the Editor instead of the numbers above:
        // Use the original _cornerPositions we grabbed in Start()
        for (int i = 0; i < jars.Length; i++)
        {
            jars[i].transform.position = _cornerPositions[i];
        }
    }
}

