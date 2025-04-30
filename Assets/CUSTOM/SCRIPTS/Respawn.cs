using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class Respawn : MonoBehaviour
{
    [Header("XR Components")]
    public Transform XROrigin;
    public Camera XRCamera;
    public CharacterController characterController;

    [Header("Settings")]
    public float fallThreshold = -5f;
    public Vector3 respawnPosition = new Vector3(0, 1.6f, 0);
    public float respawnDelay = 1f;

    private void Update()
    {
        if (XROrigin.position.y < fallThreshold)
        {
            StartCoroutine(RespawnRoutine()); // Requires System.Collections
        }
    }

    // Add the correct return type
    private IEnumerator RespawnRoutine()
{
    // 1. Disable components in correct order
    var bodyTransformer = GetComponent<XRBodyTransformer>();
    if (bodyTransformer != null) 
    {
        bodyTransformer.enabled = false;
    }

    if (characterController != null)
    {
        characterController.enabled = false;
    }

    // 2. Reset position and rotation
    InputTracking.disablePositionalTracking = true;
    XRCamera.transform.localPosition = Vector3.zero;
    XRCamera.transform.localRotation = Quaternion.identity;
    XROrigin.position = respawnPosition;

    // 3. Wait for proper frame timing
    yield return new WaitForFixedUpdate(); // Important for physics
    yield return null;

    // 4. Re-enable components in reverse order
    if (characterController != null)
    {
        characterController.enabled = true;
    }

    if (bodyTransformer != null) 
    {
        bodyTransformer.enabled = true;
    }

    InputTracking.disablePositionalTracking = false;

    // 5. Force position update
    if (characterController != null)
    {
        characterController.Move(Vector3.zero); // Clear residual movement
    }
}
}