using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapFishingRod : MonoBehaviour
{
    public Transform attachPoint;  // Reference to the desired attach transform
    public float snapSpeed = 20f;    // Speed of snapping

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        // Listen for when the object is grabbed
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Immediately move the object to the attach point
        StopAllCoroutines(); // Stop any ongoing snapping routines
        StartCoroutine(SnapToPoint());
    }

    private IEnumerator SnapToPoint()
    {
        // Optionally, you can smooth the snapping using Lerp, or simply set it immediately
        while(Vector3.Distance(transform.position, attachPoint.position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, attachPoint.position, Time.deltaTime * snapSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, attachPoint.rotation, Time.deltaTime * snapSpeed);
            yield return null;
        }
        // Final alignment
        transform.position = attachPoint.position;
        transform.rotation = attachPoint.rotation;
    }
}
