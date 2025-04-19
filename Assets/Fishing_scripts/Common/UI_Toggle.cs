using UnityEngine;
using UnityEngine.UI;

public class UI_Toggle : MonoBehaviour
{
    [Header("Panel & Buttons")]
    public GameObject ScenePanel;
    public Button ToggleButton;

    
    void Start()
    {
        ScenePanel.SetActive(false);
        ToggleButton.onClick.AddListener(ToggleUI_State);
    }

    public void ToggleUI_State()
    {
        bool open = !ScenePanel.activeSelf;
        ScenePanel.SetActive(open);
    }
}
