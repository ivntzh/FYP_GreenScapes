using UnityEngine;
using Photon.Pun;

public class Dirt : MonoBehaviour
{
    public GameObject flatDirt; // The flat dirt to hide
    public GameObject mixedDirt; // The final dirt to show
    public PotManager potManager; // Reference to PotManager script
    public AudioSource mixSound; // Sound to play when mixing
    public PhotonView photonView; // Reference to PhotonView
    private bool isMixed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isMixed) return;

        if (other.CompareTag("Rake"))
        {
            if (photonView.IsMine)
            {
                Debug.Log("Rake triggered soil mixing!");

                if (flatDirt != null) flatDirt.SetActive(false);
                if (mixedDirt != null) mixedDirt.SetActive(true);

                if (potManager != null)
                    potManager.enabled = false;

                if (mixSound != null)
                    mixSound.Play(); // Play the sound effect

                isMixed = true;

                // Notify other clients
                photonView.RPC("RPC_MixSoil", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void RPC_MixSoil()
    {
        if (flatDirt != null) flatDirt.SetActive(false);
        if (mixedDirt != null) mixedDirt.SetActive(true);
        isMixed = true;
    }
}