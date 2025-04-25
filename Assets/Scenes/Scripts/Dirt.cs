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

        // enable earthhill
        var eh = transform.root.Find("earthhill");
        if (eh != null) eh.gameObject.SetActive(true);

        isMixed = true;
    }

    public void ResetToInitial()
    {
        isMixed = false;
        if (flatDirt != null)  flatDirt.SetActive(false);
        if (mixedDirt != null) mixedDirt.SetActive(false);
        if (PotManager != null) PotManager.enabled = true;

        // disable earthhill
        var eh = transform.root.Find("earthhill");
        if (eh != null) eh.gameObject.SetActive(false);
    }
}
