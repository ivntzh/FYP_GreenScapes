using UnityEngine;

public class Dirt : MonoBehaviour
{
    public GameObject flatDirt;
    public GameObject mixedDirt;
    public MonoBehaviour PotManager;
    public AudioSource mixSound;
    private bool isMixed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isMixed || !other.CompareTag("Rake")) return;
        // Mix soil
        flatDirt.SetActive(false);
        mixedDirt.SetActive(true);
        PotManager.enabled = false;
        mixSound.Play();
        isMixed = true;
    }

    /// <summary>
    /// Disable mixed dirt, leave flatDirt off (PotManager resets it).
    /// </summary>
    public void ResetToInitial()
    {
        isMixed = false;
        mixedDirt.SetActive(false);
        PotManager.enabled = true;
    }
}
