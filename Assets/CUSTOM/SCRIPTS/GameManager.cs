using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    // A flag to ensure we only spawn one player per client.
    private bool playerSpawned = false;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        // If already in a room (for example, if you created it), attempt to spawn the player.
        if (PhotonNetwork.InRoom && !playerSpawned)
        {
            SpawnPlayer();
        }
    }

    // Called when the room is created (only on the host)
    public override void OnCreatedRoom()
    {
        if (!playerSpawned)
        {
            SpawnPlayer();
        }
    }

    // Called for both host and clients when they join a room.
    public override void OnJoinedRoom()
    {
        if (!playerSpawned)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Not in room; cannot spawn player.");
            return;
        }

        // Calculate a unique spawn position based on the player's ActorNumber.
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Vector3 spawnPosition = new Vector3(actorNumber * 2.0f, 0f, 0f);

        PhotonNetwork.Instantiate("PlayerVRPrefab", spawnPosition, Quaternion.identity);
        playerSpawned = true;
    }

    public override void OnMasterClientSwitched(Player newMaster)
    {
        // If the original host leaves, enforce leaving the room.
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("hostId", out object hostId))
        {
            if ((int)hostId != newMaster.ActorNumber)
            {
                PhotonNetwork.LeaveRoom();
            }
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void ExitGame()
    {
        if (!PhotonNetwork.InRoom) return;
        StartCoroutine(LeaveGame());
    }

    IEnumerator LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom || PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        SceneManager.LoadScene("Lobby");
    }
}
