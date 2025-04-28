using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BlockUserView : MonoBehaviour
{
    [Tooltip("Drag your full-screen black Panel here (child of this GameObject)")]
    public GameObject panel;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        panel.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 1) Call this from any static (Inspector-wired) Button’s OnClick().
    ///    It simply turns the panel on immediately.
    /// </summary>
    public void ShowBlocker()
    {
        panel.SetActive(true);
    }

    // auto-hide as soon as the next scene finishes loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        panel.SetActive(false);
    }
}
