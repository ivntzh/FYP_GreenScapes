// Dirt.cs
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

        if (flatDirt   != null) flatDirt.SetActive(false);
        if (mixedDirt  != null) mixedDirt.SetActive(true);
        if (PotManager != null) PotManager.enabled = false;
        mixSound?.Play();

        isMixed = true;
    }

    /// <summary>
    /// Restores the pre-mixed state.
    /// </summary>
    public void ResetMix()
    {
        isMixed = false;
        if (flatDirt   != null) flatDirt.SetActive(false);
        if (mixedDirt  != null) mixedDirt.SetActive(false);
        if (PotManager != null) PotManager.enabled = true;
    }
}
