using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text playerCountText;
    
    private string roomName;
    private System.Action<string> onClickAction;

    public void Initialize(RoomInfo roomInfo, System.Action<string> onClick)
    {
        roomName = roomInfo.Name;
        onClickAction = onClick;
        
        roomNameText.text = roomInfo.Name;
        playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        onClickAction?.Invoke(roomName);
    }
}
