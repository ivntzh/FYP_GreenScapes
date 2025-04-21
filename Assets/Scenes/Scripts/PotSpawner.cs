using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PotSpawner : MonoBehaviour
{
    public GameObject soilAndPlantPrefab;       // Prefab to spawn
    public Transform spawnPoint;                // Scene object for position & rotation

    private bool hasSpawned = false;

    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawned || soilAndPlantPrefab == null || spawnPoint == null) return;

        Debug.Log("🌾 Plant grabbed — spawning new soil plot");

        Instantiate(soilAndPlantPrefab, spawnPoint.position, spawnPoint.rotation);
        hasSpawned = true;
    }
}
