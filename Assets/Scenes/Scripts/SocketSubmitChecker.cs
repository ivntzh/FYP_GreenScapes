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

        // **1) Only the client who owns this particular plant** calls the RPC:
        if (PhotonNetwork.IsConnected && !submitPV.IsMine)  
            return;

        // Ask the MasterClient to handle the submission
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            submitPV.ViewID
        );

        // locally clear the socket so it’s free next time
        if (socket.GetOldestInteractableSelected() == args.interactableObject)
            socket.interactionManager.SelectExit(socket, args.interactableObject);
    }

    [PunRPC]
    public void RequestSubmitPlant(int plantViewID, PhotonMessageInfo info)
    {
        // **2) Only the MasterClient actually runs game logic**
        if (!PhotonNetwork.IsMasterClient) return;

        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;

        // Transfer ownership so we can destroy it
        submitPV.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        var plantGO = submitPV.gameObject;
        var plant   = plantGO.GetComponent<PlantType>();
        bool correct = plant != null && orderManager.CheckPlantMatch(plant.plantID);

        if (correct)
        {
            // Award once on the host
            shopManager.AddCurrency(rewardAmount);
            // **3) Sync the host’s authoritative total** back to everyone
            shopManager.SyncDataToClients();
        }

        // Remove it from the scene, network‐wide
        PhotonNetwork.Destroy(plantGO);

        // **4) Issue a new order for everyone**
        photonView.RPC(
            nameof(SyncGenerateNewOrder),
            RpcTarget.AllBuffered
        );
    }

    [PunRPC]
    void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
    }
}
