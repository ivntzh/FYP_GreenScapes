using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Photon.Pun; 

public class SocketSubmitChecker : MonoBehaviour
{
    public PlantOrderManager orderManager;
    public ShopManager shopManager;
    public int rewardAmount = 10;
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
        GameObject placedObject = args.interactableObject.transform.gameObject;
        PlantType plant = placedObject.GetComponent<PlantType>();

        if (plant != null)
        {
            bool isCorrect = orderManager.CheckPlantMatch(plant.plantID);

            if (isCorrect)
            {
                Debug.Log("✅ Correct plant submitted! Rewarding player.");
                if (shopManager != null)
                {
                    var shopView = shopManager.GetComponent<PhotonView>();

                    if (PhotonNetwork.IsConnected)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            // we’re the host, just add directly
                            shopManager.AddCurrency(rewardAmount);
                        }
                        else
                        {
                            // we’re a client: ask the host to add coins
                            shopView.RPC(
                                nameof(ShopManager.RequestAddCurrencyRPC),
                                RpcTarget.MasterClient,
                                rewardAmount
                            );
                        }
                    }
                    else
                    {
                        // offline mode
                        shopManager.AddCurrency(rewardAmount);
                    }
                }

                // ✅ Generate new order only if correct
                orderManager.GenerateNewOrder();
            }
            else
            {
                Debug.Log("❌ Wrong plant submitted. No reward. Try again.");
                // Keep same order
            }

            // Always destroy the plant
            Destroy(placedObject);

            // Clear socket manually
            socket.interactionManager.SelectExit(socket, args.interactableObject);
        }
    }
}