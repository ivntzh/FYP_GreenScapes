using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Photon.Pun;

public class SocketSubmitChecker : MonoBehaviourPun
{
    [Header("References")]
    public PlantOrderManager orderManager;
    public ShopManager       shopManager;
    public int               rewardAmount = 10;

    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnItemPlaced);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnItemPlaced);
    }

    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        var placedObject = args.interactableObject.transform.gameObject;
        var pv = placedObject.GetComponent<PhotonView>();
        if (pv == null) return;  // only handle networked plants

        // Only the owner client should request submission
        if (PhotonNetwork.IsConnected && !pv.IsMine)
            return;

        // ask the MasterClient to process submission
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            pv.ViewID
        );

        // clear local selection
        socket.interactionManager.SelectExit(socket, args.interactableObject);
    }

    [PunRPC]
    public void RequestSubmitPlant(int plantViewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;

        var plantGO = submitPV.gameObject;
        var plant = plantGO.GetComponent<PlantType>();
        bool correct = (plant != null) && orderManager.CheckPlantMatch(plant.plantID);

        if (correct)
        {
            Debug.Log("✅ Correct plant submitted! Rewarding player.");
            shopManager.AddCurrency(rewardAmount);
            shopManager.photonView.RPC(
                nameof(ShopManager.CurrencyAddedConfirmationRPC),
                RpcTarget.All,
                rewardAmount
            );
        }
        else
        {
            Debug.Log("❌ Wrong plant submitted. No reward.");
        }

        PhotonNetwork.Destroy(plantGO);
        photonView.RPC(nameof(SyncGenerateNewOrder), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
    }
}
