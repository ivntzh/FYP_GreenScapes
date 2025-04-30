using UnityEngine;

public class TutorialDirt : MonoBehaviour
{
    public GameObject flatDirt; // The flat dirt to hide
    public GameObject mixedDirt; // The final dirt to show
    public MonoBehaviour PotManager; // Script to disable
    public AudioSource mixSound; // Sound to play when mixing
    private bool isMixed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isMixed) return;

        if (other.CompareTag("Rake"))
        {
            Debug.Log("Rake triggered soil mixing!");

            if (flatDirt != null) flatDirt.SetActive(false);
            if (mixedDirt != null) mixedDirt.SetActive(true);

            if (PotManager != null)
                PotManager.enabled = false;

            if (mixSound != null)
                mixSound.Play(); // Play the sound effect

            isMixed = true;
        }
    }
}
