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
            yield return new WaitForSeconds(swapInterval - warningDuration);

            // 1. WARNING: Shake before vanishing
            float elapsed = 0f;
            while (elapsed < warningDuration)
            {
                for (int i = 0; i < jars.Length; i++)
                {
                    // FIXED: Simple shake math without the complex IndexOf call
                    Vector3 shakeOffset = (Vector3)Random.insideUnitCircle * 0.1f;
                    jars[i].transform.position = _cornerPositions[i] + shakeOffset;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 2. VANISH: Play effect and hide jars
            foreach (GameObject jar in jars)
            {
                if (poofEffectPrefab != null) Instantiate(poofEffectPrefab, jar.transform.position, Quaternion.identity);
                jar.SetActive(false); 
            }

            yield return new WaitForSeconds(0.5f); 

            // 3. RESPAWN: Move to new spots and show jars
            ShufflePositions();
            
            for (int i = 0; i < jars.Length; i++)
            {
                jars[i].SetActive(true);
                // Update our record of where the jar is currently "sitting"
                _cornerPositions[i] = jars[i].transform.position;
                if (poofEffectPrefab != null) Instantiate(poofEffectPrefab, jars[i].transform.position, Quaternion.identity);
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

