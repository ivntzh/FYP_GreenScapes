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
        // only the MasterClient ever runs this
        if (!PhotonNetwork.IsMasterClient) return;

        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;

        var plantGO = submitPV.gameObject;
        var type    = plantGO.GetComponent<PlantType>();
        bool correct = (type != null) && orderManager.CheckPlantMatch(type.plantID);

        if (correct)
        {
            // 1) Host applies the reward
            shopManager.AddCurrency(rewardAmount);

            // 2) Broadcast the new authoritative state to everyone
            shopManager.SyncDataToClients();

            // 3) Give instant feedback to ALL OTHER CLIENTS
            shopManager.photonView.RPC(
                nameof(ShopManager.CurrencyAddedConfirmationRPC),
                RpcTarget.Others,
                rewardAmount
            );

            // 4) Destroy and generate next order
            PhotonNetwork.Destroy(plantGO);
            photonView.RPC(nameof(SyncGenerateNewOrder), RpcTarget.All);
        }
        else
        {
            Debug.Log("❌ Wrong plant. No points awarded.");
        }
    }

    [PunRPC]
    void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
    }
}
