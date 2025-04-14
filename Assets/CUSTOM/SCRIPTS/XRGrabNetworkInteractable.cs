using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonRigidbodyView))]
public class NetworkRigidbodySync : MonoBehaviour { }
public class XRGrabNetworkInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private PhotonView photonView;

    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (photonView != null && !photonView.IsMine)
        {
            photonView.RequestOwnership();
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        
        if (photonView != null && photonView.IsMine)
        {
            // Transfer ownership back to the scene (master client)
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }
}