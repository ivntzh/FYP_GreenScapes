using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] TMP_InputField roomNameInput;
    [SerializeField] TMP_InputField joinPasswordInput;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] TMP_Text statusText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomButtonPrefab;
    
    private Dictionary<string, RoomInfo> cachedRooms = new Dictionary<string, RoomInfo>();
    private bool connectionReady;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        ConnectToPhoton();
    }

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            statusText.text = "Connecting...";
        }
    }

    public override void OnConnectedToMaster()
    {
        connectionReady = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "In Lobby";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRooms(roomList);
        UpdateRoomUI();
    }

    public override void OnJoinedRoom()
    {
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        PhotonNetwork.LoadLevel("Game");
        yield return new WaitForSeconds(0.1f);
    }

    public void CreateRoom()
    {
        if (!connectionReady || !PhotonNetwork.IsConnected)
        {
            statusText.text = "Not connected properly!";
            return;
        }

        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            statusText.text = "Please enter a room name!";
            return;
        }

        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = 4,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "hostId", PhotonNetwork.LocalPlayer.ActorNumber },
                { "password", passwordInput.text },
                { "hasPassword", !string.IsNullOrEmpty(passwordInput.text) }
            },
            CustomRoomPropertiesForLobby = new[] { "hostId", "hasPassword", "password" }
        };

        PhotonNetwork.CreateRoom(roomNameInput.text, options);
    }

    public void JoinRoom(string roomName)
    {
        if (cachedRooms.TryGetValue(roomName, out RoomInfo room))
        {
            bool requiresPassword = room.CustomProperties.ContainsKey("hasPassword") && (bool)room.CustomProperties["hasPassword"];
            if (requiresPassword)
            {
                if (room.CustomProperties.TryGetValue("password", out object roomPassword))
                {
                    if (joinPasswordInput.text != (string)roomPassword)
                    {
                        statusText.text = "Incorrect password!";
                        return;
                    }
                }
                else
                {
                    statusText.text = "Password not available!";
                    return;
                }
            }
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            statusText.text = "Room not found!";
        }
    }

    void UpdateCachedRooms(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                cachedRooms.Remove(room.Name);
            }
            else
            {
                cachedRooms[room.Name] = room;
            }
        }
    }

    void UpdateRoomUI()
    {
        foreach (Transform child in roomListContent)
            Destroy(child.gameObject);
        
        foreach (var room in cachedRooms.Values)
        {
            GameObject button = Instantiate(roomButtonPrefab, roomListContent);
            button.GetComponent<RoomButton>().Initialize(room, JoinRoom);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionReady = false;
        FindObjectOfType<MenuManager>()?.ShowStartMenu();
    }
}