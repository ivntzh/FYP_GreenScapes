using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))]
public class SocketSubmitChecker : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public PlantOrderManager orderManager;
    public ShopManager       shopManager;
    public int               rewardAmount = 10;

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    bool isProcessing = false;   // guard flag

    void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        socket.selectEntered.AddListener(OnItemPlaced);
    }

    void OnDestroy()
    {
        socket.selectEntered.RemoveListener(OnItemPlaced);
    }

    void OnItemPlaced(SelectEnterEventArgs args)
    {
        // 1) Guard to avoid double‐processing
        if (isProcessing) return;
        isProcessing = true;

        var go = args.interactableObject.transform.gameObject;
        var submitPV = go.GetComponent<PhotonView>();
        if (submitPV == null)
        {
            isProcessing = false;
            return;
        }

        // 2) Ask MasterClient to check & schedule destruction
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            submitPV.ViewID
        );

        // 3) Drop it from the actual interactor that selected it
        socket.interactionManager.SelectExit(
            args.interactorObject, 
            args.interactableObject
        );
    }

    [PunRPC]
    public void RequestSubmitPlant(int plantViewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var submitPV = PhotonView.Find(plantViewID);
        if (submitPV == null) return;

        var plantGO = submitPV.gameObject;
        var type    = plantGO.GetComponent<PlantType>();
        bool correct = (type != null) && orderManager.CheckPlantMatch(type.plantID);

        // ---- 4) Award & feedback via ShopManager RPCs ----
        if (correct)
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.SyncDataToClients();
            shopManager.photonView.RPC(
                nameof(ShopManager.PlayCorrectSubmissionFeedbackRPC),
                RpcTarget.All,
                rewardAmount
            );
        }
        else
        {
            shopManager.photonView.RPC(
                nameof(ShopManager.PlayWrongSubmissionFeedbackRPC),
                RpcTarget.All
            );
        }

        // ---- 5) Owner‐destroys the plant (or fallback) ----
        var owner = submitPV.Owner;
        if (owner != null)
        {
            photonView.RPC(
                nameof(RPC_DestroyPlant),
                owner,
                plantViewID
            );
        }
        else
        {
            // Fallback if the owner has left
            PhotonNetwork.Destroy(plantGO);
            // Now that it's gone, spawn next order
            photonView.RPC(
                nameof(SyncGenerateNewOrder),
                RpcTarget.All
            );
        }
    }

    [PunRPC]
    void RPC_DestroyPlant(int plantViewID, PhotonMessageInfo info)
    {
        var pv = PhotonView.Find(plantViewID);
        if (pv != null && pv.IsMine)
        {
            PhotonNetwork.Destroy(pv.gameObject);
            // Only *after* destroy do we tell everyone to make the next order
            photonView.RPC(
                nameof(SyncGenerateNewOrder),
                RpcTarget.All
            );
        }
    }

    [PunRPC]
    void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
        isProcessing = false;
    }
}
