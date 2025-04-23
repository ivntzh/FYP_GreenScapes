using UnityEngine;
using Photon.Pun; // Add Photon namespace

public class Xylophone_audio : MonoBehaviourPun // Inherit from MonoBehaviourPun
{
    public AudioClip Xylo_AudioClip;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("baqueta")) return;
    
        if (photonView.IsMine) // Only the local player triggers
        {
            // Play locally immediately for responsiveness
            LocalPlaySound();
        
            // Tell others to play it
            photonView.RPC("PlaySoundRPC", RpcTarget.Others);
        }
    }

    [PunRPC]
    private void PlaySoundRPC()
    {
        // Only remote clients execute this
        if (!photonView.IsMine)
        {
            LocalPlaySound();
        }
    }

    private void LocalPlaySound()
    {
        if (Xylo_AudioClip == null)
        {
            Debug.LogError("Audioclip missing!");
            return;
        }
        
        AudioSource.PlayClipAtPoint(Xylo_AudioClip, transform.position);
    }
}