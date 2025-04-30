// Dirt.cs
using UnityEngine;
using Photon.Pun;

public class Dirt : MonoBehaviourPun
{
    [Header("Flat vs Mixed Dirt (children of this)")]
    public GameObject flatDirt;    // your FlatDirt child
    public GameObject mixedDirt;   // your MixedDirt or earth-hill mesh child

    [Header("EarthHill Root")]
    [Tooltip("Drag your EarthHill GameObject here (the one with SoilGrowthOnParticle)")]
    public GameObject earthHillRoot;

    [Header("Dependencies")]
    public PotManager potManager;  // The script that spawns flatDirt
    public AudioSource mixSound;

    private bool isMixed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isMixed || !other.CompareTag("Rake")) return;

        // Broadcast the mix to everyone, buffered
        photonView.RPC(nameof(RPC_MixDirt), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_MixDirt()
    {
        // 1) Hide flat dirt, show mixed dirt
        flatDirt?.SetActive(false);
        mixedDirt?.SetActive(true);

        // 2) Enable the EarthHill root so the soil-growth script runs
        earthHillRoot?.SetActive(true);
        // ¡ú *and* immediately turn its MeshRenderer back on:
        var mr = earthHillRoot.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = true;

        // 3) Disable PotManager so no more dirt spawns until reset
        potManager.enabled = false;

        // 4) Sound & flag
        mixSound?.Play();
        isMixed = true;
    }

    /// <summary>
    /// Called by SoilGrowthOnParticle when the final©\plant harvest resets the pot.
    /// </summary>
    public void ResetToInitial()
    {
        isMixed = false;
        flatDirt?.SetActive(false);
        mixedDirt?.SetActive(false);
        potManager.enabled = true;

        // Hide the earth hill again until the next rake
        earthHillRoot?.SetActive(false);
    }
}
