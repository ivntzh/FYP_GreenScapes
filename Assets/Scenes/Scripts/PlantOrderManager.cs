using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlantOrderManager : MonoBehaviourPun
{
    public string[] possibleOrders = { "Plant", "Cactus" };
    public string currentOrderID;
    public TextMeshProUGUI orderText;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GenerateNewOrder();
        }
    }

    public void GenerateNewOrder()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentOrderID = possibleOrders[Random.Range(0, possibleOrders.Length)];
        photonView.RPC("SyncOrderToClients", RpcTarget.AllBuffered, currentOrderID);
    }

    [PunRPC]
    private void SyncOrderToClients(string newOrder)
    {
        currentOrderID = newOrder;
        if (orderText != null)
            orderText.text = $"📝 Order: {currentOrderID}";
    }

    public bool CheckPlantMatch(string plantID)
    {
        return plantID == currentOrderID;
    }
}
