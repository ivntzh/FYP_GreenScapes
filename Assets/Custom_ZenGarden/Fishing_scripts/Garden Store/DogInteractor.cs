using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DogInteraction : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;
    public string[] animationTriggers;
    
    [Header("Dialogue Settings")]
    public string[] dialogueLines;
    public TextMeshProUGUI dialogueText;
    public float dialogueDuration = 3f;
    
    [Header("Interaction Settings")]
    public XRGrabInteractable grabInteractable;

    private void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to the select-enter event that passes event args
        grabInteractable.selectEntered.AddListener(OnDogGrab);
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnDogGrab);
    }

    // Note: use the correct EventArgs type for your XRIT version
    private void OnDogGrab(SelectEnterEventArgs args)
    {
        // Trigger a random animation
        if (animationTriggers.Length > 0)
        {
            var trigger = animationTriggers[Random.Range(0, animationTriggers.Length)];
            animator?.SetTrigger(trigger);
        }

        // Show random dialogue
        if (dialogueLines.Length > 0 && dialogueText != null)
        {
            var line = dialogueLines[Random.Range(0, dialogueLines.Length)];
            StopAllCoroutines();
            StartCoroutine(ShowDialogueRoutine(line));
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
