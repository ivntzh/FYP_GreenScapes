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
    bool isProcessing = false;   // ← guard flag

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
        // 1) If we’re already handling a placement, ignore
        if (isProcessing) return;
        isProcessing = true;

        var go = args.interactableObject.transform.gameObject;
        var submitPV = go.GetComponent<PhotonView>();
        if (submitPV == null)
        {
            isProcessing = false;  // nothing to do
            return;
        }

        // 2) Ask the MasterClient to submit
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            submitPV.ViewID
        );

        // 3) Free the grab so hand can re-pick
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
        bool correct = (type != null) && orderManager.CheckPlantMatch(type.plantID);

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
            Debug.Log("❌ Wrong plant submitted. No points awarded.");
            shopManager.photonView.RPC(
                nameof(ShopManager.PlayWrongSubmissionFeedbackRPC),
                RpcTarget.All
            );
        }

        // 4) Transfer ownership & destroy on the MC
        submitPV.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        PhotonNetwork.Destroy(plantGO);

        // 5) Generate next order on all clients—and reset the flag there
        photonView.RPC(nameof(SyncGenerateNewOrder), RpcTarget.All);
    }

    [PunRPC]
    void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
        // 6) Now that we’re ready for the next plant, clear the guard
        isProcessing = false;
    }
}
