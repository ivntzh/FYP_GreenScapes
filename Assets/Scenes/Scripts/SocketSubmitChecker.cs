using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Photon.Pun;

[RequireComponent(typeof(XRSocketInteractor))]
public class SocketSubmitChecker : MonoBehaviourPunCallbacks
{
    public PlantOrderManager orderManager;
    public ShopManager       shopManager;
    public int               rewardAmount = 10;

    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    void OnEnable() => socket.selectEntered.AddListener(OnItemPlaced);
    void OnDisable() => socket.selectEntered.RemoveListener(OnItemPlaced);

    void OnItemPlaced(SelectEnterEventArgs args)
    {
        var go = args.interactableObject.transform.gameObject;
        var pv = go.GetComponent<PhotonView>();
        if (pv == null) return;

        photonView.RPC(nameof(RequestSubmitPlant), RpcTarget.MasterClient, pv.ViewID);

        if (socket.GetOldestInteractableSelected() == args.interactableObject)
            socket.interactionManager.SelectExit(socket, args.interactableObject);
    }

    [PunRPC]
    public void RequestSubmitPlant(int plantViewID, PhotonMessageInfo info)
    {
        // Only the MasterClient does this
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;

        // 1) Transfer ownership of the plant view to the MasterClient
        //    so we become its owner and can destroy it
        submitPV.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        // 2) Evaluate correctness & award if needed…
        var plantGO = submitPV.gameObject;
        var plant   = plantGO.GetComponent<PlantType>();
        bool correct = plant != null && orderManager.CheckPlantMatch(plant.plantID);
        if (correct)
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.photonView.RPC(
                nameof(ShopManager.CurrencyAddedConfirmationRPC),
                RpcTarget.All,
                rewardAmount
            );
        }

        // 3) Now that the MasterClient *owns* it, we can destroy it network-wide
        PhotonNetwork.Destroy(plantGO);
    }
}