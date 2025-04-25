// SocketSubmitChecker.cs
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

    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        if (socket == null)
            Debug.LogError("SocketSubmitChecker needs an XRSocketInteractor");
    }

    private void OnEnable()  => socket?.selectEntered.AddListener(OnItemPlaced);
    private void OnDisable() => socket?.selectEntered.RemoveListener(OnItemPlaced);

    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        var placedGO = args.interactableObject.transform.gameObject;
        var pv       = placedGO.GetComponent<PhotonView>();
        if (pv == null) return;

        // always ask the MasterClient to process
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            pv.ViewID
        );

        // free the grab
        if (socket.GetOldestInteractableSelected() == args.interactableObject)
            socket.interactionManager.SelectExit(socket, args.interactableObject);
    }

    [PunRPC]
    public void RequestSubmitPlant(int plantViewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;

        var plantGO = submitPV.gameObject;
        var plant   = plantGO.GetComponent<PlantType>();
        bool correct = (plant != null) && orderManager.CheckPlantMatch(plant.plantID);

        if (correct)
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.photonView.RPC(
                nameof(ShopManager.CurrencyAddedConfirmationRPC),
                RpcTarget.All,
                rewardAmount
            );
        }

        // only MasterClient ever calls Destroy
        PhotonNetwork.Destroy(plantGO);

        // local-cleanup on others
        photonView.RPC(
            nameof(RPC_LocalDestroyPlant),
            RpcTarget.OthersBuffered,
            plantViewID
        );

        // immediately generate next order
        orderManager.GenerateNewOrder();
    }

    [PunRPC]
    void RPC_LocalDestroyPlant(int plantViewID)
    {
        var pv = PhotonView.Find(plantViewID);
        if (pv != null) Destroy(pv.gameObject);
    }
}
