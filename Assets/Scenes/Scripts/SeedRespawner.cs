using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class SeedRespawner : MonoBehaviourPun
{
    [Tooltip("Must match a prefab in Resources/")]
    public string seedPrefabName;

    private Vector3   spawnPosition;
    private Quaternion spawnRotation;
    private bool hasSpawnedNew = false;

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    // Hook this up in the Inspector to XRGrabInteractable ¡ú On Select Entered
    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawnedNew) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // Host spawns it
            photonView.RPC(nameof(RPC_DoSpawnSeed), RpcTarget.AllBuffered);
        }
        else
        {
            // Clients ask host
            photonView.RPC(nameof(RPC_RequestSpawnSeed), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    public void RPC_RequestSpawnSeed(PhotonMessageInfo info)
    {
        // Only master should handle
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC(nameof(RPC_DoSpawnSeed), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_DoSpawnSeed()
    {
        // Instantiate the seed prefab for everyone
        Instantiate(Resources.Load<GameObject>(seedPrefabName),
                    spawnPosition,
                    spawnRotation);
        hasSpawnedNew = true;
    }
}
