using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class PotSpawner : MonoBehaviour
{
    public GameObject potPrefab;       // This same prefab or new version
    public RecyclingManager recyclingManager; // Reference to RecyclingManager
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool hasSpawnedNew = false;

    void Start()
    {
        // Cache the original spawn position & rotation
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void Initialize()
    {
        hasSpawnedNew = false;
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawnedNew || potPrefab == null) return;

        if (recyclingManager.photonView.IsMine)
        {
            Debug.Log("🪴 Pot grabbed — spawning new one");

            Instantiate(potPrefab, spawnPosition, spawnRotation);
            hasSpawnedNew = true;

            // Notify other clients
            recyclingManager.photonView.RPC(
                nameof(RecyclingManager.RPC_SpawnPot),
                RpcTarget.All,
                spawnPosition,
                spawnRotation
            );
        }
    }

    [PunRPC]
    private void RPC_SpawnPot(Vector3 position, Quaternion rotation)
    {
        Instantiate(potPrefab, position, rotation);
        hasSpawnedNew = true;
    }
}