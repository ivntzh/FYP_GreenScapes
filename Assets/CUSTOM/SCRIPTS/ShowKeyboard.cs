using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System.Collections;

[RequireComponent(typeof(TMP_InputField))]
public class ShowKeyboard : MonoBehaviour
{
    public enum PositionMode { FollowTransform, StaticPosition }

    [Header("Position Settings")]
    [SerializeField] PositionMode positionMode = PositionMode.FollowTransform;
    [SerializeField] Transform positionSource;
    [SerializeField] float distance = 0.5f;
    [SerializeField] float verticalOffset = 0.1f;
    [SerializeField] Vector3 staticPosition = new Vector3(0, -0.5f, 1f);
    [SerializeField] Vector3 staticRotation = new Vector3(0, 180f, 0);

    [Header("Keyboard Reference")]
    [SerializeField] NonNativeKeyboard keyboardPrefab;

    private TMP_InputField inputField;
    private static NonNativeKeyboard keyboardInstance;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onSelect.AddListener(_ => OpenKeyboard());
        InitializeKeyboard();
    }

    void InitializeKeyboard()
    {
        if (keyboardInstance != null || keyboardPrefab == null) return;

        // Create keyboard in hidden space first
        keyboardInstance = Instantiate(keyboardPrefab, Vector3.down * 1000f, Quaternion.identity);
        keyboardInstance.gameObject.SetActive(true);
        keyboardInstance.gameObject.SetActive(false);
    }

    void OpenKeyboard()
    {
        if (keyboardInstance == null)
        {
            Debug.LogError("Keyboard not initialized! Assign prefab in inspector.");
            return;
        }

        StartCoroutine(ActivateKeyboard());
    }

    IEnumerator ActivateKeyboard()
    {
        // Ensure keyboard is ready
        keyboardInstance.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();

        // Configure keyboard
        keyboardInstance.InputField = inputField;
        keyboardInstance.PresentKeyboard(inputField.text);

        // Set position/rotation
        if (positionMode == PositionMode.StaticPosition)
        {
            keyboardInstance.transform.SetPositionAndRotation(
                staticPosition, 
                Quaternion.Euler(staticRotation)
            );
        }
        else
        {
            Vector3 targetPos = positionSource.position + 
                              positionSource.forward * distance + 
                              Vector3.up * verticalOffset;
                              
            keyboardInstance.RepositionKeyboard(targetPos, positionSource.eulerAngles.y);
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
    }
}