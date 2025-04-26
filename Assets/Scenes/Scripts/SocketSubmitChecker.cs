using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Photon.Pun;

[RequireComponent(typeof(XRSocketInteractor))]
public class SocketSubmitChecker : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public PlantOrderManager orderManager;
    public ShopManager       shopManager;
    public int               rewardAmount = 10;

    XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnItemPlaced);
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnItemPlaced);
    }

    void OnItemPlaced(SelectEnterEventArgs args)
    {
        var go = args.interactableObject.transform.gameObject;
        var submitPV = go.GetComponent<PhotonView>();
        if (submitPV == null) return;

        // 🔒 only the client that actually owns this plant should ask to submit
        if (PhotonNetwork.IsConnected && !submitPV.IsMine)
            return;

        // send *one* request to the MasterClient
        photonView.RPC(
          nameof(RequestSubmitPlant),
          RpcTarget.MasterClient,
          submitPV.ViewID
        );

        // un-select locally
        if (socket.GetOldestInteractableSelected() == args.interactableObject)
            socket.interactionManager.SelectExit(socket, args.interactableObject);
    }

    [PunRPC]
    public void RequestSubmitPlant(int plantViewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;

        // 1) Make *sure* the MasterClient owns it
        submitPV.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        // 2) Award coins once
        var plantGO = submitPV.gameObject;
        var plant   = plantGO.GetComponent<PlantType>();
        if (plant != null && orderManager.CheckPlantMatch(plant.plantID))
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.SyncDataToClients();
        }

        // 3) Now that we own it, destroy it network‐wide
        PhotonNetwork.Destroy(plantGO);

        // 4) New order for everyone
        photonView.RPC(nameof(SyncGenerateNewOrder), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
    }
}
