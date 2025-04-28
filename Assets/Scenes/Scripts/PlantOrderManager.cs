using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlantOrderManager : MonoBehaviourPun
{
    [Header("Order Settings")]
    public string[] possibleOrders = { "Plant", "Cactus" };
    public string   currentOrderID;

    [Header("UI Order Images")]
    public RawImage plantOrderImage;
    public RawImage cactusOrderImage;

    void Start()
    {
        // Only the host generates a new order
        if (PhotonNetwork.IsMasterClient)
            GenerateNewOrder();
    }

    /// <summary>
    /// Host picks a random order and syncs it to all clients.
    /// </summary>
    public void GenerateNewOrder()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentOrderID = possibleOrders[Random.Range(0, possibleOrders.Length)];
        photonView.RPC(
            nameof(SyncOrderToClients),
            RpcTarget.AllBuffered,
            currentOrderID
        );
    }

    [PunRPC]
    void SyncOrderToClients(string newOrder)
    {
        // Everyone (host + clients) runs this
        currentOrderID = newOrder;
        UpdateOrderUI();
    }

    void UpdateOrderUI()
    {
        // Turn both off, then enable the one matching currentOrderID
        if (plantOrderImage != null)  plantOrderImage.enabled  = false;
        if (cactusOrderImage != null) cactusOrderImage.enabled = false;

        if (currentOrderID == "Plant" && plantOrderImage != null)
            plantOrderImage.enabled = true;
        else if (currentOrderID == "Cactus" && cactusOrderImage != null)
            cactusOrderImage.enabled = true;
    }

    /// <summary>
    /// Used by your socket‐submit checker to validate a dropped plant.
    /// </summary>
    public bool CheckPlantMatch(string plantID)
    {
        return plantID == currentOrderID;
    }
}
