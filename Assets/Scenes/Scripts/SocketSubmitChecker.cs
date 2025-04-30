using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(XRSocketInteractor))]
public class SocketSubmitChecker : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public PlantOrderManager orderManager;
    public ShopManager       shopManager;
    public int               rewardAmount = 10;

    XRSocketInteractor socket;
    bool isProcessing = false;   // guard flag

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
        // 1) Prevent double‐processing
        if (isProcessing) return;
        isProcessing = true;

        var go = args.interactableObject.transform.gameObject;
        var plantType = go.GetComponent<PlantType>();
        if (plantType == null)
        {
            // Not a plant: immediately drop it so it can be re-grabbed
            socket.interactionManager.SelectExit(
                args.interactorObject,
                args.interactableObject
            );
            isProcessing = false;
            return;
        }

        // 2) We know it's a PlantType, now grab its PhotonView
        var submitPV = go.GetComponent<PhotonView>();
        if (submitPV == null)
        {
            // somehow un-networked? just drop it
            socket.interactionManager.SelectExit(
                args.interactorObject,
                args.interactableObject
            );
            isProcessing = false;
            return;
        }

        // 3) Notify the host to validate, award, destroy & next‐order
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            submitPV.ViewID
        );

        // 4) Un-select locally so hand can re-pick
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

        // 5) Award & feedback
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

        // 6) Destroy the plant on its owner (or fallback)
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
            PhotonNetwork.Destroy(plantGO);
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
