using UnityEngine;

public class PlantType : MonoBehaviour
{
    public string plantID; // Set to "Plant", "Cactus", or "Unknown" in Inspector
    public RecyclingManager recyclingManager; // Reference to RecyclingManager

    private void Start()
    {
        // Initialize plant type
        recyclingManager.InitializePlantType(this);
    }
}