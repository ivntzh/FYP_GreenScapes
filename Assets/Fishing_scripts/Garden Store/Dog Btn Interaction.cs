using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DogBtnInteraction : MonoBehaviour
{
    [Header("Dog Setup")]
    [Tooltip("The dog GameObject’s Animator")]
    public Animator dogAnimator;
    [Tooltip("Names of the Animator triggers to fire (must match exactly).")]
    public string[] animationTriggers = new string[]
    {
    };

    [Header("Dialogue Setup")]
    [Tooltip("Lines the dog can “say”.")]
    [TextArea]
    public string[] dialogueLines;
    [Tooltip("UI Text element for showing the dog’s dialogue.")]
    public TextMeshProUGUI dialogueText;
    [Tooltip("How long (seconds) the dialogue stays on screen.")]
    public float dialogueDuration = 3f;

    [Header("Audio Setup")]
    [Tooltip("AudioSource that will play the bark SFX.")]
    public AudioSource barkAudioSource;
    [Tooltip("One-shot bark sound.")]
    public AudioClip barkClip;

    [Header("UI Button")]
    [Tooltip("The UI Button that the player presses to “talk” to the dog.")]
    public Button storeButton;

    private void Awake()
    {
        // Validate references
        if (storeButton == null)
            Debug.LogError("StoreButton not assigned!", this);
        if (dogAnimator == null)
            Debug.LogError("Dog Animator not assigned!", this);
        if (dialogueText == null)
            Debug.LogError("Dialogue Text not assigned!", this);
        if (barkAudioSource == null)
            Debug.LogError("Bark AudioSource not assigned!", this);

        // Hide dialogue at start
        dialogueText.gameObject.SetActive(false);

        // Hook up the button
        storeButton.onClick.AddListener(OnStoreButtonPressed);
    }

    private void OnDestroy()
    {
        // Clean up listener
        storeButton.onClick.RemoveListener(OnStoreButtonPressed);
    }

    /// <summary>
    /// Call this from the UI Button’s OnClick() event.
    /// </summary>
    public void OnStoreButtonPressed()
    {
        Debug.Log("Dog is interacted!");
        // 1) Trigger a random dog animation
        if (animationTriggers != null && animationTriggers.Length > 0)
        {
            int idx = Random.Range(0, animationTriggers.Length);
            Debug.Log("This is trigger: " + idx + "The length: " + animationTriggers.Length);
            string trigger = animationTriggers[idx];
            dogAnimator.SetTrigger(trigger);
            Debug.Log("This is trigger: " + trigger);
        }

        // 2) Show a random dialogue line
        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            int idx = Random.Range(0, dialogueLines.Length);
            string line = dialogueLines[idx];
            StopAllCoroutines();
            StartCoroutine(ShowDialogueRoutine(line));
            Debug.Log("This is dialogue: " + line);
        }

        // 3) Play bark sound
        if (barkAudioSource != null && barkClip != null)
        {
            barkAudioSource.PlayOneShot(barkClip);
        }
    }

    private IEnumerator ShowDialogueRoutine(string line)
    {
        dialogueText.text = line;
        dialogueText.gameObject.SetActive(true);
        yield return new WaitForSeconds(dialogueDuration);
        dialogueText.gameObject.SetActive(false);
    }
}
