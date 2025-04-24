using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlantOrderManager : MonoBehaviourPunCallbacks
{
    public string[] possibleOrders = { "Plant", "Cactus" };
    public string currentOrderID;
    public TextMeshProUGUI orderText;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
                GenerateAndSyncOrder();
        }
        else
        {
            GenerateNewOrder();
        }
    }

    public void RequestNewOrder()
    {
        if (!PhotonNetwork.IsConnected)
        {
            GenerateNewOrder();
            return;
        }

        if (PhotonNetwork.IsMasterClient)
            GenerateAndSyncOrder();
        else
            photonView.RPC(nameof(GenerateOrderRPC), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void GenerateOrderRPC(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GenerateAndSyncOrder();
    }

    private void GenerateAndSyncOrder()
    {
        GenerateNewOrder();
        photonView.RPC(nameof(SyncOrderRPC), RpcTarget.OthersBuffered, currentOrderID);
    }

    [PunRPC]
    private void SyncOrderRPC(string id)
    {
        currentOrderID = id;
        if (orderText != null)
            orderText.text = $"📝 Order: {currentOrderID}";
    }

    public void GenerateNewOrder()
    {
        currentOrderID = possibleOrders[Random.Range(0, possibleOrders.Length)];
        if (orderText != null)
            orderText.text = $"📝 Order: {currentOrderID}";
    }

    public bool CheckPlantMatch(string plantID)
    {
        return plantID == currentOrderID;
    }
}