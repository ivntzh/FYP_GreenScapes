using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public float updateInterval = 0.1f; // Update every 0.1 seconds
    private float timeSinceLastUpdate = 0f;

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;

            if (cameraTransform == null)
            {
                if (Camera.main != null)
                    cameraTransform = Camera.main.transform;
                else
                    return;
            }

            Vector3 direction = transform.position - cameraTransform.position;
            direction.y = 0; // Optional: Keep the canvas upright by zeroing the Y component

            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
