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
    public RecyclingManager recyclingManager; // Reference to RecyclingManager
    public PhotonView photonView; // Reference to PhotonView

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    public void Initialize()
    {
        // Initialize socket events
        socket.selectEntered.AddListener(OnItemPlaced);
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

                if (recyclingManager != null)
                {
                    recyclingManager.SubmitPlant(plant.plantID);
                }
            }
            else
            {
                Debug.Log("❌ Wrong plant submitted. No reward. Try again.");
            }

            // Always destroy the plant
            Destroy(placedObject);

            // Clear socket manually
            socket.interactionManager.SelectExit(socket, args.interactableObject);
        }
    }
}