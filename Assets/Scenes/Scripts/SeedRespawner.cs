using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class SeedRespawner : MonoBehaviourPun
{
    public GameObject seedPrefab;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool hasSpawnedNew = false;

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawnedNew || seedPrefab == null) return;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
                SpawnSeed();
            else
                photonView.RPC(nameof(RequestSpawnSeedRPC), RpcTarget.MasterClient);
        }
        else
        {
            SpawnSeed();
        }
    }

    [PunRPC]
    private void RequestSpawnSeedRPC(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        SpawnSeed();
        photonView.RPC(nameof(SyncSpawnSeedRPC), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    private void SyncSpawnSeedRPC()
    {
        SpawnSeed();
    }

    private void SpawnSeed()
    {
        hasSpawnedNew = true;
        // use PhotonNetwork.Instantiate if seedPrefab is a registered network prefab
        PhotonNetwork.Instantiate(seedPrefab.name, spawnPosition, spawnRotation);
        Debug.Log("Seed respawned.");
    }
}
