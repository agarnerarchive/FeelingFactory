using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 _originalPos;
    private Coroutine _currentShake;

    void Awake() => _originalPos = transform.localPosition;

    public void Shake(float duration, float magnitude)
    {
        // If a shake is already happening, stop it so they don't stack weirdly
        if (_currentShake != null) StopCoroutine(_currentShake);
        _currentShake = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalPos;
        _currentShake = null;
    }
}

