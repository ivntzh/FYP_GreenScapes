using UnityEngine;
using UnityEngine.InputSystem;

public class VRInputHandler : MonoBehaviour
{
    public InputActionReference castAction; // Bind to the cast button
    public InputActionReference pullAction; // Bind to the pull button

    public bool IsCastTriggered()
    {
        return castAction.action.triggered;
    }

    public bool IsPullTriggered()
    {
        return pullAction.action.triggered;
    }
}
