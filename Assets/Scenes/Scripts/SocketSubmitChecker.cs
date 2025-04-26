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
        socket.selectEntered.AddListener(OnItemPlaced);
    }

    void OnDestroy()
    {
        socket.selectEntered.RemoveListener(OnItemPlaced);
    }

    void OnItemPlaced(SelectEnterEventArgs args)
    {
        var go = args.interactableObject.transform.gameObject;
        var submitPV = go.GetComponent<PhotonView>();
        if (submitPV == null) return;

        // Any client can place; ask the MasterClient to process it
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            submitPV.ViewID
        );

        // free the grab so hand can re-pick
        if (socket.GetOldestInteractableSelected() == args.interactableObject)
            socket.interactionManager.SelectExit(socket, args.interactableObject);
    }

    [PunRPC]
    public void RequestSubmitPlant(int plantViewID, PhotonMessageInfo info)
    {
        // 1) Only the MasterClient ever runs this
        if (!PhotonNetwork.IsMasterClient) return;

        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;
        var plantGO = submitPV.gameObject;

        // 2) Check correctness
        var type = plantGO.GetComponent<PlantType>();
        bool correct = (type != null) && orderManager.CheckPlantMatch(type.plantID);
        if (!correct)
        {
            Debug.Log("❌ Wrong plant. No points awarded.");
            return;
        }

        // 3) Award currency on host
        shopManager.AddCurrency(rewardAmount);

        // 4) Sync the authoritative shop state (currency, purchases) to all clients
        shopManager.SyncDataToClients();

        // 5) Show the “+X Coins!” toast on *everyone*, including the host
        shopManager.photonView.RPC(
            nameof(ShopManager.CurrencyAddedConfirmationRPC),
            RpcTarget.All,
            rewardAmount
        );

        // 6) Guarantee the MasterClient owns the plant before destroying it, so that
        // after a host switch the new MasterClient can still kill it.
        submitPV.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        // 7) Now destroy network‐wide (always called on the MC)
        PhotonNetwork.Destroy(plantGO);

        // 8) Generate the next order on everyone
        photonView.RPC(nameof(SyncGenerateNewOrder), RpcTarget.All);
    }

    [PunRPC]
    void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
    }
}
