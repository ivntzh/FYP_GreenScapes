using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;

[RequireComponent(typeof(TMP_InputField))]
public class ShowKeyboard : MonoBehaviour
{
    public enum PositionMode { FollowTransform, StaticPosition }

    [Header("Keyboard Settings")]
    [SerializeField] NonNativeKeyboard keyboardPrefab; // Assign in inspector
    [SerializeField] PositionMode positionMode = PositionMode.FollowTransform;

    [Header("Dynamic Positioning")]
    [SerializeField] Transform positionSource;
    [SerializeField] float distance = 0.5f;
    [SerializeField] float verticalOffset = 0.1f;

    [Header("Static Positioning")]
    [SerializeField] Vector3 staticPosition = new Vector3(0, -0.5f, 1f);
    [SerializeField] Vector3 staticRotation = new Vector3(0, 180f, 0);

    private TMP_InputField inputField;
    private NonNativeKeyboard keyboardInstance;
    private bool isInitializationComplete;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onSelect.AddListener(_ => OpenKeyboard());
        StartCoroutine(InitializeKeyboard());
    }

    IEnumerator InitializeKeyboard()
    {
        if (keyboardPrefab == null)
        {
            Debug.LogError("Keyboard prefab not assigned in inspector!");
            yield break;
        }

        // Find existing instance including inactive
        keyboardInstance = FindObjectOfType<NonNativeKeyboard>(true);

        if (keyboardInstance == null)
        {
            // Create new instance
            keyboardInstance = Instantiate(keyboardPrefab);
            
            // Force initialization
            keyboardInstance.gameObject.SetActive(true);
            yield return null; // Wait one frame for Awake/Start
            
            // Set static position if needed
            if (positionMode == PositionMode.StaticPosition)
            {
                keyboardInstance.transform.position = staticPosition;
                keyboardInstance.transform.rotation = Quaternion.Euler(staticRotation);
            }
            
            keyboardInstance.gameObject.SetActive(false);
        }

        isInitializationComplete = true;
    }

    void OpenKeyboard()
    {
        if (!isInitializationComplete || keyboardInstance == null)
        {
            Debug.LogError($"Keyboard initialization failed. Ready: {isInitializationComplete} Instance: {keyboardInstance != null}");
            return;
        }

        StartCoroutine(ActivateKeyboard());
    }

    IEnumerator ActivateKeyboard()
    {
        keyboardInstance.gameObject.SetActive(true);
        yield return null; // Wait for components to initialize

        // Configure keyboard
        keyboardInstance.InputField = inputField;
        keyboardInstance.PresentKeyboard(inputField.text);

        // Set position
        switch (positionMode)
        {
            case PositionMode.FollowTransform:
                if (positionSource != null)
                {
                    Vector3 pos = positionSource.position + 
                                positionSource.forward * distance + 
                                Vector3.up * verticalOffset;
                    keyboardInstance.RepositionKeyboard(pos);
                }
                break;

            case PositionMode.StaticPosition:
                keyboardInstance.transform.position = staticPosition;
                keyboardInstance.transform.rotation = Quaternion.Euler(staticRotation);
                break;
        }

        keyboardInstance.OnClosed += (s, e) => 
        {
            keyboardInstance.gameObject.SetActive(false);
            inputField.DeactivateInputField();
        };
    }

    void OnDestroy()
    {
        if (inputField != null)
            inputField.onSelect.RemoveAllListeners();
        
        if (keyboardInstance != null)
            Destroy(keyboardInstance.gameObject);
    }
}