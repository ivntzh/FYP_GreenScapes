using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_visibility : MonoBehaviour
{
    [Header("References")]
    // Instead of a camera, assign an empty GameObject that's a child of your XR rig.
    public Transform playerReference;    
    public Transform targetObject;       // The gameobject to check against.
    public GameObject uiCanvas;          // The Canvas holding your game UI.

    [Header("Visibility Settings")]
    public float maxDistance = 5f;      // Maximum distance for UI to remain visible.
    public float maxAngle = 100f;         // Maximum angle (in degrees) for the player's view toward the target.


    private void Update()
    {

        // Calculate the distance between the player's reference and the target.
        float distance = Vector3.Distance(playerReference.position, targetObject.position);

        // Calculate the direction to the target and determine the angle between this direction and the player's forward.
        Vector3 directionToTarget = (targetObject.position - playerReference.position).normalized;
        float angle = Vector3.Angle(playerReference.forward, directionToTarget);

        // Activate or deactivate the UI based on the distance and the angle.
        if (distance <= maxDistance && angle <= maxAngle)
        {
            uiCanvas.SetActive(true);
        }
        else
        {
            uiCanvas.SetActive(false);
        }
    }
}
