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

        flatDirt?.SetActive(false);
        mixedDirt?.SetActive(true);
        PotManager?.enabled = false;
        mixSound?.Play();
        isMixed = true;
    }

    /// <summary>
    /// Call this to restore the pre-mixed state.
    /// </summary>
    public void ResetMix()
    {
        isMixed = false;
        flatDirt?.SetActive(true);
        mixedDirt?.SetActive(false);
        PotManager?.enabled = true;
    }
}
