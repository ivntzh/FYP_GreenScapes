using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using XR.Interaction.Toolkit.Samples;

public class PlayerNetwork : MonoBehaviourPun
{
    private void Start()
    {
        Debug.Log($"Initializing player network object. IsMine: {photonView.IsMine}");

        if (photonView.IsMine)
        {
            Debug.Log("Configuring local player");
            ConfigureLocalPlayer();
        }
        else
        {
            Debug.Log("Configuring remote player");
            DisableRemotePlayerComponents();
        }
    }

    void Update()
    {
        if (!photonView.IsMine || !Application.isFocused) return;
        ProcessInput();
    }

    void ProcessInput()
    {
        // Your input processing logic here
    }

    private void DisableRemotePlayerComponents()
    {
        Debug.Log("Starting remote player component disable");
        
        // Audio Listeners
        var audioListeners = GetComponentsInChildren<AudioListener>();
        foreach (var listener in audioListeners)
        {
            listener.enabled = false;
            Debug.Log($"Disabled AudioListener on {listener.gameObject.name}");
        }

        // Movement Gravity
        var dynamicMove = GetComponentInChildren<DynamicMoveProvider>(true);
        if (dynamicMove != null)
        {
            Debug.Log($"Found DynamicMoveProvider. Current gravity: {dynamicMove.useGravity}");
            dynamicMove.useGravity = false;
            Debug.Log("Disabled gravity on DynamicMoveProvider");
        }
        else
        {
            Debug.LogWarning("Could not find DynamicMoveProvider for remote player");
        }

        // Controllers
        DisableComponentsWithLog<XRController>("XRController");
        DisableComponentsWithLog<ActionBasedController>("ActionBasedController");

        // Interactors
        var interactors = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(true);
        foreach (var interactor in interactors)
        {
            interactor.enabled = false;
            Debug.Log($"Disabled XRBaseInteractor on {interactor.gameObject.name}");
        }

        Debug.Log("Completed remote player component disable");
    }

    private void ConfigureLocalPlayer()
    {
        Debug.Log("Starting local player configuration");
        
        // Ensure XR Origin is active
        var xrOrigin = GetComponent<XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.gameObject.SetActive(true);
            Debug.Log("Activated XROrigin");
        }

        // Audio Listeners
        foreach (var listener in GetComponentsInChildren<AudioListener>())
        {
            listener.enabled = true;
            Debug.Log($"Enabled AudioListener on {listener.gameObject.name}");
        }

        // Movement Gravity
        var dynamicMove = GetComponentInChildren<DynamicMoveProvider>(true);
        if (dynamicMove != null)
        {
            Debug.Log($"Found DynamicMoveProvider. Current gravity: {dynamicMove.useGravity}");
            dynamicMove.useGravity = true;
            Debug.Log("Enabled gravity on DynamicMoveProvider");
        }
        else
        {
            Debug.LogWarning("Could not find DynamicMoveProvider for local player");
        }

        // Enable controllers
        EnableComponentsWithLog<XRController>("XRController");
        EnableComponentsWithLog<ActionBasedController>("ActionBasedController");

        // Interactors
        var interactors = GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(true);
        foreach (var interactor in interactors)
        {
            interactor.enabled = true;
            Debug.Log($"Enabled XRBaseInteractor on {interactor.gameObject.name}");
        }

        Debug.Log("Completed local player configuration");
    }

    private void DisableComponentsWithLog<T>(string componentName) where T : Behaviour
    {
        var components = GetComponentsInChildren<T>(true);
        foreach (var component in components)
        {
            component.enabled = false;
            Debug.Log($"Disabled {componentName} on {component.gameObject.name}");
        }
    }

    private void EnableComponentsWithLog<T>(string componentName) where T : Behaviour
    {
        var components = GetComponentsInChildren<T>(true);
        foreach (var component in components)
        {
            component.enabled = true;
            Debug.Log($"Enabled {componentName} on {component.gameObject.name}");
        }
    }
}