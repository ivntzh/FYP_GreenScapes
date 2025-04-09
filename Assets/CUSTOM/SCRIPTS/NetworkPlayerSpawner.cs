using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private bool hasSpawned = false;

    private void Start()
    {
        // Check if already in a room when the component starts (e.g., after scene load)
        if (PhotonNetwork.InRoom && !hasSpawned)
        {
            SpawnPlayer();
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (!hasSpawned)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Vector3 spawnPosition = new Vector3(actorNumber * 2.0f, 0f, 0f);
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("PlayerVRPrefab", spawnPosition, Quaternion.identity);
        Debug.Log("Spawned prefab for actor " + actorNumber + ". PhotonView.IsMine: " + spawnedPlayerPrefab.GetComponent<PhotonView>().IsMine);
        hasSpawned = true;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        if (spawnedPlayerPrefab != null)
        {
            PhotonNetwork.Destroy(spawnedPlayerPrefab);
        }
        hasSpawned = false;
    }
}