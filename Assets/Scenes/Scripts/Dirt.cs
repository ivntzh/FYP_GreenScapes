// Dirt.cs
using UnityEngine;
using Photon.Pun;

public class Dirt : MonoBehaviourPunCallbacks
{
    [Header("Flat vs Mixed Dirt")]
    public GameObject flatDirt;    // Drag your ¡°flat dirt¡± child here
    public GameObject mixedDirt;   // Drag your ¡°earthhill¡± child here

    [Header("Dependencies")]
    public PotManager potManager;  // The script that puts flatDirt in
    public AudioSource mixSound;

    bool isMixed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isMixed || !other.CompareTag("Rake")) return;
        // Do the mix on everyone (buffered)
        photonView.RPC(nameof(RPC_MixDirt), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_MixDirt()
    {
        flatDirt?.SetActive(false);
        mixedDirt?.SetActive(true);
        potManager.enabled = false;  // lock out further dirt drops
        mixSound?.Play();
        isMixed = true;
    }

    /// <summary>
    /// Called by SoilGrowthOnParticle once final-plant cycle finishes.
    /// Returns dirt to its un-mixed state.
    /// </summary>
    public void ResetToInitial()
    {
        isMixed = false;
        flatDirt?.SetActive(false);
        mixedDirt?.SetActive(false);
        potManager.enabled = true;
    }
}
