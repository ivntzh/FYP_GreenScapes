using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private GameObject lobbyCanvas;

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;

    void Start()
    {
        ShowStartMenu();
    }

    public void OnStartButtonClicked()
    {
        ShowLobbyMenu();
        networkManager.ConnectToPhoton();
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }

    public void ShowStartMenu()
    {
        startCanvas.SetActive(true);
        lobbyCanvas.SetActive(false);
    }

    public void ShowLobbyMenu()
    {
        startCanvas.SetActive(false);
        lobbyCanvas.SetActive(true);
    }
}