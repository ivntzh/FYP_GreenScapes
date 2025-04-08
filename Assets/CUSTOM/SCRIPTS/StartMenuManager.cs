using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("Lobby"); // Load Lobby scene
    }

    public void OnExitButtonClicked()
    {
        Application.Quit(); // Works in builds, not in editor
    }

    public void OnMuteButtonClicked()
    {
        AudioListener.volume = (AudioListener.volume == 0) ? 1 : 0; // Toggle audio
    }
}