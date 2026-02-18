using UnityEngine;

public class ConstantSpinner : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Degrees per second around each axis")]
    public Vector3 rotationSpeed = new Vector3(0, 100, 0);

    [Tooltip("Use Local or World space?")]
    public Space space = Space.Self;

    void Update()
    {
        // Multiply by Time.deltaTime to ensure smooth, frame-rate independent motion
        transform.Rotate(rotationSpeed * Time.deltaTime, space);
    }
}
