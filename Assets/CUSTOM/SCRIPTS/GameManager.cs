using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    private bool isLeaving;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnMasterClientSwitched(Player newMaster)
    {
        if (PhotonNetwork.CurrentRoom == null || !PhotonNetwork.InRoom) return;
        
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("hostId", out object hostId))
        {
            Debug.LogError("hostId not found in room properties!");
            return;
        }
        
        if ((int)hostId != newMaster.ActorNumber && PhotonNetwork.IsConnected)
        {
            StartCoroutine(SafeLeaveRoom());
        }
    }

    public void ExitGame()
    {
        if (!PhotonNetwork.InRoom || isLeaving) return;
        StartCoroutine(SafeLeaveRoom());
    }

    IEnumerator SafeLeaveRoom()
    {
        if (isLeaving) yield break;
        isLeaving = true;
        
        if (PhotonNetwork.InRoom && PhotonNetwork.Server == ServerConnection.GameServer)
        {
            PhotonNetwork.LeaveRoom();
            yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        }

        if (PhotonNetwork.IsConnected)
        {
            yield return new WaitUntil(() => PhotonNetwork.Server == ServerConnection.MasterServer);
            PhotonNetwork.Disconnect();
            yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        }

        LoadLobbyUI();
        isLeaving = false;
    }

    void LoadLobbyUI()
    {
        if (SceneManager.GetActiveScene().name != "StartMenu")
        {
            SceneManager.LoadScene("StartMenu");
        }
        StartCoroutine(ReinitializeNetwork());
    }

    IEnumerator ReinitializeNetwork()
    {
        yield return new WaitForSeconds(0.1f);
        NetworkManager netManager = FindObjectOfType<NetworkManager>();
        netManager?.ConnectToPhoton();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (SceneManager.GetActiveScene().name != "StartMenu")
        {
            // Explicitly destroy when disconnecting
            Destroy(gameObject);
            LoadLobbyUI();
        }
    }
}