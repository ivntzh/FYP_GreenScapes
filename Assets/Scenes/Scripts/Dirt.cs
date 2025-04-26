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
        if (flatDirt != null)    flatDirt.SetActive(false);
        if (mixedDirt != null)   mixedDirt.SetActive(true);
        if (PotManager != null)  PotManager.enabled = false;
        if (mixSound != null)    mixSound.Play();

        // enable earthhill root on mix
        var earthHill = transform.root.Find("earthhill");
        if (earthHill != null)  earthHill.gameObject.SetActive(true);

        isMixed = true;
    }

    /// <summary>Restore to initial, pre-dirt state (no earth).</summary>
    public void ResetToInitial()
    {
        isMixed = false;
        if (flatDirt != null)    flatDirt.SetActive(false);
        if (mixedDirt != null)   mixedDirt.SetActive(false);
        if (PotManager != null)  PotManager.enabled = true;
    }
}