// ScreenShake.cs
// Attach to your Main Camera. Uses local position offset for shake.
 
using UnityEngine;
using System.Collections;
 
public class ScreenShake : MonoBehaviour
{
    private Vector3 originalLocalPosition;
    private Coroutine activeShake;
 
    private void Start()
    {
        originalLocalPosition = transform.localPosition;
    }
 
    /// <param name="duration">How long the shake lasts (seconds)</param>
    /// <param name="magnitude">Intensity of shake (pixels in UI space)</param>
    public void Shake(float duration, float magnitude)
    {
        // Stop any running shake so they don't stack awkwardly
        if (activeShake != null) StopCoroutine(activeShake);
        activeShake = StartCoroutine(ShakeRoutine(duration, magnitude));
    }
 
    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
 
        while (elapsed < duration)
        {
            // Dampen shake over time
            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            float offsetX  = Random.Range(-1f, 1f) * strength;
            float offsetY  = Random.Range(-1f, 1f) * strength;
 
            transform.localPosition = originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
 
        // Restore exact original position
        transform.localPosition = originalLocalPosition;
    }
}


