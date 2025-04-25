using UnityEngine;
using Photon.Pun;

public class RecyclingManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public ShopManager shopManager;              // Reference to the ShopManager
    public PlantOrderManager plantOrderManager;  // Reference to the PlantOrderManager
    public ShovelManager shovelManager;         // Reference to the ShovelManager
    public PotManager potManager;               // Reference to the PotManager
    public WateringCan wateringCan;             // Reference to the WateringCan
    public SocketSubmitChecker socketSubmitChecker; // Reference to the SocketSubmitChecker

    private PhotonView photonView;               // Reference to PhotonView

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        // Initialize all managers
        InitializeManagers();
    }

    void InitializeManagers()
    {
        // Initialize PlantOrderManager
        plantOrderManager.Initialize();

        // Initialize ShovelManager
        shovelManager.Initialize();

        // Initialize PotManager
        potManager.Initialize();

        // Initialize WateringCan
        wateringCan.Initialize();

        // Initialize SocketSubmitChecker
        socketSubmitChecker.Initialize();
    }

    // Example method to handle plant submission
    [PunRPC]
    public void SubmitPlant(string plantID)
    {
        if (photonView.IsMine)
        {
            // Validate plant submission
            bool isCorrect = plantOrderManager.CheckPlantMatch(plantID);

            if (isCorrect)
            {
                // Reward player
                shopManager.AddCurrency(10);

                // Generate new order
                plantOrderManager.GenerateNewOrder();

                // Notify all clients
                photonView.RPC(
                    nameof(RPC_SubmitPlant),
                    RpcTarget.All,
                    plantID,
                    isCorrect
                );
            }
            else
            {
                // Notify all clients
                photonView.RPC(
                    nameof(RPC_SubmitPlant),
                    RpcTarget.All,
                    plantID,
                    isCorrect
                );
            }
        }
    }

    [PunRPC]
    private void RPC_SubmitPlant(string plantID, bool isCorrect)
    {
        // Handle plant submission on all clients
        if (isCorrect)
        {
            Debug.Log("✅ Correct plant submitted!");
            plantOrderManager.GenerateNewOrder();
        }
        else
        {
            Debug.Log("❌ Wrong plant submitted.");
        }
    }

    [PunRPC]
    public void RPC_MixSoil()
    {
        // Handle soil mixing on all clients
        Debug.Log("Soil mixed!");
    }

    [PunRPC]
    public void RPC_DirtPlaced()
    {
        // Handle dirt placement on all clients
        Debug.Log("Dirt placed in pot!");
    }

    [PunRPC]
    public void RPC_SpawnPot(Vector3 position, Quaternion rotation)
    {
        // Spawn pot on all clients
        Debug.Log("Pot spawned!");
    }

    [PunRPC]
    public void RPC_SpawnSeed(Vector3 position, Quaternion rotation)
    {
        // Spawn seed on all clients
        Debug.Log("Seed spawned!");
    }

    [PunRPC]
    public void RPC_PickupDirt()
    {
        // Handle dirt pickup on all clients
        Debug.Log("Dirt picked up!");
    }

    [PunRPC]
    public void RPC_DropDirt()
    {
        // Handle dirt dropping on all clients
        Debug.Log("Dirt dropped!");
    }

    [PunRPC]
    public void RPC_StartPouring()
    {
        // Handle pouring start on all clients
        Debug.Log("Pouring started!");
    }

    [PunRPC]
    public void RPC_StopPouring()
    {
        // Handle pouring stop on all clients
        Debug.Log("Pouring stopped!");
    }

    public void InitializePlantType(PlantType plantType)
    {
        // Initialize plant type
        Debug.Log("Plant type initialized: " + plantType.plantID);
    }

    public void InitializeTrashType(TrashType trashType)
    {
        // Initialize trash type
        Debug.Log("Trash type initialized: " + trashType.trashCategory);
    }
}