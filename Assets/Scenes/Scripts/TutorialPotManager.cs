using UnityEngine;

public class TutorialPotManager : MonoBehaviour
{
    public GameObject flatDirt; // Assign this in Inspector
    public AudioSource dirtDropSound; // Assign this in Inspector

    private void Start()
    {
        if (flatDirt != null)
            flatDirt.SetActive(false); // Make sure it's hidden at start
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DroppedDirt"))
        {
            Debug.Log("Dirt detected in pot!");

            if (flatDirt != null)
                flatDirt.SetActive(true); // Show the flat dirt

            if (dirtDropSound != null)
                dirtDropSound.Play(); // Play sound on trigger

            Destroy(other.gameObject); // Optional: destroy dropped dirt

            this.enabled = false; // Disable script after triggered
        }
    }
}
