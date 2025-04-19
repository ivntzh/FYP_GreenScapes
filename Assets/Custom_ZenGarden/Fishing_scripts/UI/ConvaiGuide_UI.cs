using UnityEngine;
using UnityEngine.UI;

public class ConvaiGuide_UI : MonoBehaviour
{
    [Header("Panel & Buttons")]
    public GameObject uiCanvas;          // The Canvas holding your game UI.
    public GameObject btnCanvas;          // The Canvas holding btn UI.
    public Button HelpButton;
    public Button DismissButton;

    [Header("References")]
    // Instead of a camera, assign an empty GameObject that's a child of your XR rig.
    public Transform playerReference;    
    public Transform targetObject;       // The gameobject to check against.

    [Header("Visibility Settings")]
    public float maxDistance = 5f;      // Maximum distance for UI to remain visible.
    public float maxAngle = 100f;         // Maximum angle (in degrees) for the player's view toward the target.
    bool toggledOn = false;

    
    void Start()
    {
        uiCanvas.SetActive(false);
        HelpButton.onClick.AddListener(OpenUI_State);
        DismissButton.onClick.AddListener(CloseUI_State);
    }

    private void Update()
    {

        // Calculate the distance between the player's reference and the target.
        float distance = Vector3.Distance(playerReference.position, targetObject.position);

        // Calculate the direction to the target and determine the angle between this direction and the player's forward.
        Vector3 directionToTarget = (targetObject.position - playerReference.position).normalized;
        float angle = Vector3.Angle(playerReference.forward, directionToTarget);

        // Activate or deactivate the UI based on the distance and the angle.
        if (distance <= maxDistance && angle <= maxAngle && toggledOn == true)
        {
            uiCanvas.SetActive(true);
            btnCanvas.SetActive(false);
        }
        else if (distance <= maxDistance && angle <= maxAngle && toggledOn == false)
        {
            uiCanvas.SetActive(false);
            btnCanvas.SetActive(true);
        }
        else
        {
            uiCanvas.SetActive(false);
            btnCanvas.SetActive(false);
        }
    }

    public void OpenUI_State()
    {    
        toggledOn = true;
        uiCanvas.SetActive(true);
        btnCanvas.SetActive(false);
    }

    public void CloseUI_State()
    {    
        toggledOn = false;
        uiCanvas.SetActive(false);
        btnCanvas.SetActive(true);
    }

}
