using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(XRGrabInteractable))]
public class SeatInteractable : MonoBehaviour
{
    [Header("Seat Setup")]
    public XRGrabInteractable interactable;    // must be on this GameObject
    public Transform          seatPoint;       // where the camera lands

    [Header("Locomotion Providers")]
    public TeleportationProvider    teleportationProvider;
    public ContinuousMoveProvider   continuousMoveProvider;

    [Header("Sit/Stand Settings")]
    [Tooltip("Distance (m) to auto‑stand when you walk off.")]
    public float standThreshold = 0.5f;

    bool    isSitting     = false;
    Vector3 seatedAtPos;
    Vector3 savedLocalPos;
    bool    localPosFrozen = false;

    void Reset()
    {
        if (interactable == null)           interactable = GetComponent<XRGrabInteractable>();
        if (teleportationProvider == null)  teleportationProvider = FindFirstObjectByType<TeleportationProvider>();
        if (continuousMoveProvider == null) continuousMoveProvider = FindFirstObjectByType<ContinuousMoveProvider>();
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSitRequested);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSitRequested);
    }

    void OnSitRequested(SelectEnterEventArgs args)
    {
        if (!isSitting)
            Sit();

        // immediately clear the grab so you can sit again
        var manager = interactable.interactionManager;
        if (manager != null)
            manager.CancelInteractableSelection((IXRSelectInteractable)interactable);
    }

    void Sit()
    {
        if (seatPoint == null || teleportationProvider == null)
            return;

        // teleport rig so camera lands at seatPoint
        var req = new TeleportRequest {
            destinationPosition = seatPoint.position,
            destinationRotation = Quaternion.LookRotation(seatPoint.forward, Vector3.up),
            matchOrientation    = MatchOrientation.WorldSpaceUp
        };
        teleportationProvider.QueueTeleportRequest(req);

        // record seat position
        seatedAtPos = seatPoint.position;
        isSitting   = true;

        // freeze camera local pos
        var cam = Camera.main.transform;
        savedLocalPos    = cam.localPosition;
        cam.localPosition = Vector3.zero;
        localPosFrozen   = true;

        // keep continuous‑move enabled so player can walk off
        // if (continuousMoveProvider != null)
        //     continuousMoveProvider.enabled = true;
        continuousMoveProvider.enabled = false; // disable movement so that player is fixed in position unless button is pressed
    }

    void Update()
    {
        if (!isSitting || !localPosFrozen) return;

        var cam = Camera.main.transform;
        cam.localPosition = Vector3.zero;  // lock at seat height

        // auto‑stand when you move away
        if (Vector3.Distance(cam.position, seatedAtPos) > standThreshold)
            StandUp();
    }

    void StandUp()
    {
        isSitting = false;

        if (localPosFrozen)
        {
            Camera.main.transform.localPosition = savedLocalPos;
            localPosFrozen = false;
        }
        // continuousMoveProvider remains enabled
    }
}
