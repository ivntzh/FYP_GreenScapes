using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Photon.Pun;  // ← need this for PhotonNetwork

public class SocketSubmitChecker : MonoBehaviour
{
    public PlantOrderManager orderManager;
    public ShopManager       shopManager;
    public int               rewardAmount = 10;

    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnItemPlaced);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnItemPlaced);
    }

    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        var placedObject = args.interactableObject.transform.gameObject;
        var plant        = placedObject.GetComponent<PlantType>();
        if (plant == null) return;

        bool isCorrect = orderManager.CheckPlantMatch(plant.plantID);
        if (isCorrect)
        {
            Debug.Log("✅ Correct plant submitted!");

            // --- Multiplayer currency award ---
            if (shopManager != null)
            {
                if (PhotonNetwork.IsConnected)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // Host applies directly
                        shopManager.AddCurrency(rewardAmount);
                    }
                    else
                    {
                        // Client requests host to add
                        shopManager.photonView.RPC(
                            "RequestAddCurrencyRPC",
                            RpcTarget.MasterClient,
                            rewardAmount
                        );
                    }
                }
                else
                {
                    // Offline fallback
                    shopManager.AddCurrency(rewardAmount);
                }
            }

            // Generate a new order
            orderManager.GenerateNewOrder();
        }
        else
        {
            Debug.Log("❌ Wrong plant submitted. No reward. Try again.");
            // No change to the order
        }

        // --- Destroy the submitted plant ---
        var pv = placedObject.GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.IsConnected)
            PhotonNetwork.Destroy(placedObject);
        else
            Destroy(placedObject);

        // Clear the socket selection
        socket.interactionManager.SelectExit(socket, args.interactableObject);
    }
}
