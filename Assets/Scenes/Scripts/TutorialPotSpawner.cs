using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialPotSpawner : MonoBehaviour
{
    public GameObject potPrefab;       // This same prefab or new version
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool hasSpawnedNew = false;

    void Start()
    {
        // Cache the original spawn position & rotation
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawnedNew || potPrefab == null) return;

        Debug.Log("🪴 Pot grabbed — spawning new one");

        Instantiate(potPrefab, spawnPosition, spawnRotation);
        hasSpawnedNew = true;
    }
}
