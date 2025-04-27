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
        if (isProcessing) return;
        isProcessing = true;

        var go = args.interactableObject.transform.gameObject;
        var submitPV = go.GetComponent<PhotonView>();
        if (submitPV == null)
        {
            isProcessing = false;
            return;
        }

        // 1) Ask MasterClient to check & schedule destruction
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            submitPV.ViewID
        );

        // 2) Always drop the item so hand is free
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
        bool correct = (type != null) && orderManager.CheckPlantMatch(type.plantID);

        // ---- Award & feedback via ShopManager RPCs ----
        if (correct)
        {
            Debug.Log("✅ Correct plant! Awarding points.");
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
            Debug.Log("❌ Wrong plant submitted.");
            shopManager.photonView.RPC(
                nameof(ShopManager.PlayWrongSubmissionFeedbackRPC),
                RpcTarget.All
            );
        }

        // ---- Owner‐destroys the plant ----
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
            // Fallback: if owner left, MasterClient destroys
            PhotonNetwork.Destroy(plantGO);
        }

        // ---- Next order & reset guard everywhere ----
        photonView.RPC(
            nameof(SyncGenerateNewOrder),
            RpcTarget.All
        );
    }

    [PunRPC]
    void RPC_DestroyPlant(int plantViewID, PhotonMessageInfo info)
    {
        var pv = PhotonView.Find(plantViewID);
        if (pv != null && pv.IsMine)
        {
            PhotonNetwork.Destroy(pv.gameObject);
        }
    }

    [PunRPC]
    void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
        isProcessing = false;
    }
}
