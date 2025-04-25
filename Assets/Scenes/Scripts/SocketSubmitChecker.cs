// SocketSubmitChecker.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Photon.Pun;

[RequireComponent(typeof(XRSocketInteractor))]
public class SocketSubmitChecker : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public PlantOrderManager   orderManager;
    public ShopManager         shopManager;
    public SoilGrowthOnParticle growthManager;
    public int                 rewardAmount = 10;

    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        if (socket == null)
            Debug.LogError("SocketSubmitChecker needs an XRSocketInteractor on the same GameObject");
    }

    private void OnEnable()
    {
        socket?.selectEntered.AddListener(OnItemPlaced);
    }

    private void OnDisable()
    {
        socket?.selectEntered.RemoveListener(OnItemPlaced);
    }

    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        var placedGO = args.interactableObject.transform.gameObject;
        var pv       = placedGO.GetComponent<PhotonView>();
        if (pv == null) return;   // only networked plants

        // — point 8: always use RPC to host, even if we're the host —
        photonView.RPC(
            nameof(RequestSubmitPlant),
            RpcTarget.MasterClient,
            pv.ViewID
        );

        // clear selection so it can be re-grabbed
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

        // 3) Authoritatively destroy the networked plant
        PhotonNetwork.Destroy(plantGO);

        // 4) — point 9: only tell other clients to locally destroy —
        photonView.RPC(
            nameof(RPC_LocalDestroyPlant),
            RpcTarget.OthersBuffered,
            plantViewID
        );

        // 5) Reset the pot & generate a new order (no buffering)
        photonView.RPC(
            nameof(SyncGenerateNewOrder),
            RpcTarget.All
        );
        if (growthManager != null)
        {
            growthManager.photonView.RPC(
                nameof(SoilGrowthOnParticle.RPC_ResetGrowth),
                RpcTarget.All
            );
        }
    }

    [PunRPC]
    void RPC_LocalDestroyPlant(int plantViewID)
    {
        var pv = PhotonView.Find(plantViewID);
        if (pv != null)
            Destroy(pv.gameObject);
    }

    [PunRPC]
    public void SyncGenerateNewOrder()
    {
        orderManager.GenerateNewOrder();
    }
}
