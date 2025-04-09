using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnMasterClientSwitched(Player newMaster)
    {
        if (PhotonNetwork.CurrentRoom == null) return;
        
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("hostId", out object hostId))
        {
            Debug.LogError("hostId not found in room properties!");
            return;
        }
        
        if ((int)hostId != newMaster.ActorNumber)
        {
            PhotonNetwork.LeaveRoom();
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