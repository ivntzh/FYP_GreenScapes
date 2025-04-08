using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

using Hashtable = ExitGames.Client.Photon.Hashtable;

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

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Starting NetworkManager: Attempting to connect to Photon.");
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            statusText.text = "Connecting...";
            Debug.Log("Connecting to Photon...");
        }
        else
        {
            Debug.LogWarning("Already connected to Photon.");
        }
    }

    // ===== Photon Callbacks =====
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");
        PhotonNetwork.JoinLobby();
        statusText.text = "In Lobby";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"Room list updated. {roomList.Count} rooms available.");
        UpdateCachedRooms(roomList);
        UpdateRoomUI();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("You are the Master Client. Loading Game scene...");
            PhotonNetwork.LoadLevel("Game");
        }
    }

    public override void OnMasterClientSwitched(Player newMaster)
    {
        Debug.Log($"Master client switched to: {newMaster.NickName} (ID: {newMaster.ActorNumber})");
    
        if (PhotonNetwork.CurrentRoom == null) return;
    
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("hostId", out object hostId))
        {
            if (newMaster.ActorNumber != (int)hostId)
            {
                Debug.Log("New master is not the original host. Leaving room gracefully...");
                StartCoroutine(LeaveRoomGracefully());
            }
        }
    }

    IEnumerator LeaveRoomGracefully()
    {
        Debug.Log("Leaving room...");
        PhotonNetwork.LeaveRoom();
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        Debug.Log("Successfully left the room. Returning to Lobby.");
        SceneManager.LoadScene("Lobby");
    }

    // ===== Room Management =====
    public void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            Debug.LogWarning("Room creation failed: Room name is empty.");
            statusText.text = "Please enter a room name!";
            return;
        }

        Debug.Log($"Creating room: {roomNameInput.text} with password: {(string.IsNullOrEmpty(passwordInput.text) ? "No" : "Yes")}");

        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable
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
        Debug.Log($"Attempting to join room: {roomName}");

        if (cachedRooms.TryGetValue(roomName, out RoomInfo room))
        {
            bool requiresPassword = room.CustomProperties.ContainsKey("hasPassword") && (bool)room.CustomProperties["hasPassword"];
            if (requiresPassword)
            {
                if (room.CustomProperties.TryGetValue("password", out object roomPassword))
                {
                    if (joinPasswordInput.text != (string)roomPassword)
                    {
                        Debug.LogWarning("Incorrect password entered.");
                        statusText.text = "Incorrect password!";
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Room requires a password, but no password found in properties!");
                    statusText.text = "Password not available!";
                    return;
                }
            }
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogWarning("Room not found in cached list.");
            statusText.text = "Room not found!";
        }
    }

    void UpdateCachedRooms(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                Debug.Log($"Removing room from cache: {room.Name}");
                cachedRooms.Remove(room.Name);
            }
            else
            {
                Debug.Log($"Updating cached room: {room.Name}");
                cachedRooms[room.Name] = room;
            }
        }
    }

    void UpdateRoomUI()
    {
        Debug.Log("Updating Room UI...");

        foreach (Transform child in roomListContent)
            Destroy(child.gameObject);
        
        foreach (var room in cachedRooms.Values)
        {
            GameObject button = Instantiate(roomButtonPrefab, roomListContent);
            button.GetComponent<RoomButton>().Initialize(room, JoinRoom);
            Debug.Log($"Added room to UI: {room.Name}");
        }
    }

    public void LeaveRoom()
    {
        Debug.Log("Leaving current room...");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Successfully left room. Loading Lobby scene.");
        SceneManager.LoadScene("Lobby");
    }

    public void Disconnect()
    {
        Debug.Log("Disconnecting from Photon...");
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon. Reason: {cause}");
        SceneManager.LoadScene("StartMenu");
    }
}
