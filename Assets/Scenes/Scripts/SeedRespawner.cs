using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class SeedRespawner : MonoBehaviourPun
{
    [Tooltip("Must match a prefab in Resources/PhotonPrefabs/")]
    public string seedPrefabName;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool hasSpawnedNew = false;

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    // Hook this in XRGrabInteractable ¡ú On Select Entered
    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawnedNew) return;

        // No local instantiate! Always ask the MasterClient to handle it:
        photonView.RPC(
            nameof(RPC_RequestSpawnSeed),
            RpcTarget.MasterClient
        );
    }

    [PunRPC]
    void RPC_RequestSpawnSeed(PhotonMessageInfo info)
    {
        // Only the MasterClient ever spawns
        if (!PhotonNetwork.IsMasterClient || hasSpawnedNew) return;

        // Network©\spawn the seed once
        PhotonNetwork.Instantiate(
            seedPrefabName,
            spawnPosition,
            spawnRotation
        );

        hasSpawnedNew = true;
    }
}
