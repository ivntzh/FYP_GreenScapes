using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlantOrderManager : MonoBehaviour
{
    public string[] possibleOrders = { "Plant", "Cactus"};
    public string currentOrderID;
    public TextMeshProUGUI orderText;
    public RecyclingManager recyclingManager; // Reference to RecyclingManager
    public PhotonView photonView; // Reference to PhotonView

    public void Initialize()
    {
        GenerateNewOrder();
    }

    public void GenerateNewOrder()
    {
        currentOrderID = possibleOrders[Random.Range(0, possibleOrders.Length)];
        if (orderText != null)
            orderText.text = $"📝 Order: {currentOrderID}";

        // Notify other clients
        photonView.RPC(
            nameof(RPC_UpdateOrder),
            RpcTarget.All,
            currentOrderID
        );
    }

    [PunRPC]
    private void RPC_UpdateOrder(string newOrderID)
    {
        currentOrderID = newOrderID;
        if (orderText != null)
            orderText.text = $"📝 Order: {currentOrderID}";
    }

    public bool CheckPlantMatch(string plantID)
    {
        return plantID == currentOrderID;
    }
}