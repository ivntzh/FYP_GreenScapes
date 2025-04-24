using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class PotSpawner : MonoBehaviourPun
{
    public GameObject potPrefab;
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
        if (hasSpawnedNew || potPrefab == null) return;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
                SpawnPot();
            else
                photonView.RPC(nameof(RequestSpawnPotRPC), RpcTarget.MasterClient);
        }
        else
        {
            SpawnPot();
        }
    }

    [PunRPC]
    public void RequestSpawnPotRPC(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        SpawnPot();
        photonView.RPC(nameof(SyncSpawnPotRPC), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    public void SyncSpawnPotRPC()
    {
        SpawnPot();
    }

    private void SpawnPot()
    {
        hasSpawnedNew = true;
        PhotonNetwork.Instantiate(potPrefab.name, spawnPosition, spawnRotation);
        Debug.Log("Pot spawned for next use.");
    }
}