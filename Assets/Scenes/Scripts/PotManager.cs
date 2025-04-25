using UnityEngine;
using Photon.Pun;

public class PotManager : MonoBehaviour
{
    public GameObject flatDirt; // Assign this in Inspector
    public AudioSource dirtDropSound; // Assign this in Inspector
    public RecyclingManager recyclingManager; // Reference to RecyclingManager
    public PhotonView photonView; // Reference to PhotonView

    private void Start()
    {
        if (flatDirt != null)
            flatDirt.SetActive(false); // Make sure it's hidden at start
    }

    public void Initialize()
    {
        // Initialize pot manager
        Debug.Log("PotManager initialized.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DroppedDirt"))
        {
            if (photonView.IsMine)
            {
                Debug.Log("Dirt detected in pot!");

                if (flatDirt != null)
                    flatDirt.SetActive(true); // Show the flat dirt

                if (dirtDropSound != null)
                    dirtDropSound.Play(); // Play sound on trigger

                Destroy(other.gameObject); // Optional: destroy dropped dirt

                this.enabled = false; // Disable script after triggered

                // Notify other clients
                recyclingManager.photonView.RPC(
                    nameof(RecyclingManager.RPC_DirtPlaced),
                    RpcTarget.All
                );
            }
        }
    }

    [PunRPC]
    private void RPC_DirtPlaced()
    {
        if (flatDirt != null)
            flatDirt.SetActive(true);
        this.enabled = false;
    }
}