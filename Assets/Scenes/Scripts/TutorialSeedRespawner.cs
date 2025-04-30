using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialSeedRespawner : MonoBehaviour
{
    public GameObject seedPrefab;   // Reference to this same prefab
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private bool hasSpawnedNew = false;

    void Start()
    {
        // Record the original position and rotation
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawnedNew || seedPrefab == null) return;

        Instantiate(seedPrefab, spawnPosition, spawnRotation);
        hasSpawnedNew = true; // Prevents multiple spawns
    }
}
