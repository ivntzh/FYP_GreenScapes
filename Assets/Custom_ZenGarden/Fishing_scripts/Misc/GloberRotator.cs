using UnityEngine;

public class GlobeRotator : MonoBehaviour
{
    [Tooltip("Degrees per second around the Y axis")]
    public float rotationSpeed = 10f;

    void Update()
    {
        // Rotate around world-up (Y) at rotationSpeed degrees per second
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
