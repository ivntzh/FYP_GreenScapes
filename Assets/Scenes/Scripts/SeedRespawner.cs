using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class SeedRespawner : MonoBehaviour
{
    public GameObject seedPrefab;   // Reference to this same prefab
    public PhotonView photonView;   // Reference to PhotonView
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

        if (photonView.IsMine)
        {
            Instantiate(seedPrefab, spawnPosition, spawnRotation);
            hasSpawnedNew = true; // Prevents multiple spawns

            // Notify other clients
            photonView.RPC("RPC_SpawnSeed", RpcTarget.All, spawnPosition, spawnRotation);
        }
    }

    [PunRPC]
    private void RPC_SpawnSeed(Vector3 position, Quaternion rotation)
    {
        Instantiate(seedPrefab, position, rotation);
        hasSpawnedNew = true;
    }
}