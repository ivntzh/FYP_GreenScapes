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
        if (!PhotonNetwork.IsMasterClient) return;
        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;
        var plantGO = submitPV.gameObject;
        var type = plantGO.GetComponent<PlantType>();
        bool correct = type != null && orderManager.CheckPlantMatch(type.plantID);
        if (correct)
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.photonView.RPC(nameof(ShopManager.CurrencyAddedConfirmationRPC), RpcTarget.All, rewardAmount);
        }
        else
        {
            Debug.Log("❌ Wrong plant. No points awarded.");
        }
        PhotonNetwork.Destroy(plantGO);
    }
}