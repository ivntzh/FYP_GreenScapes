using UnityEngine;
using Photon.Pun;

public class ShovelManager : MonoBehaviourPun
{
    public GameObject dirtOnShovel;
    public GameObject droppedDirtPrefab;
    public ParticleSystem dirtPickupParticles;
    public AudioSource pickupSound;

    private bool hasDirt = false;

    void Start()
    {
        if (dirtOnShovel != null)
            dirtOnShovel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasDirt && other.CompareTag("DirtPile"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsMasterClient)
                    ProcessPickup();
                else
                    photonView.RPC(nameof(RequestPickupRPC), RpcTarget.MasterClient);
            }
            else
            {
                ProcessPickup();
            }
        }
    }

    [PunRPC]
    private void RequestPickupRPC(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ProcessPickup();
        photonView.RPC(nameof(SyncPickupRPC), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    private void SyncPickupRPC()
    {
        ProcessPickup();
    }

    private void ProcessPickup()
    {
        hasDirt = true;
        if (dirtOnShovel != null)
            dirtOnShovel.SetActive(true);
        dirtPickupParticles?.Play();
        pickupSound?.Play();
        Debug.Log("Shovel picked up dirt.");
    }

    // Call this when the user uses the shovel to drop dirt
    public void OnActivate()
    {
        if (!hasDirt) return;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
                ProcessDrop();
            else
                photonView.RPC(nameof(RequestDropRPC), RpcTarget.MasterClient);
        }
        else
        {
            ProcessDrop();
        }
    }

    [PunRPC]
    private void RequestDropRPC(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ProcessDrop();
        photonView.RPC(nameof(SyncDropRPC), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    private void SyncDropRPC()
    {
        ProcessDrop();
    }

    private void ProcessDrop()
    {
        hasDirt = false;
        dirtOnShovel?.SetActive(false);

        if (droppedDirtPrefab != null)
        {
            // use networked instantiate so others see it too
            PhotonNetwork.Instantiate(
                droppedDirtPrefab.name,
                transform.position,
                transform.rotation
            );
        }
    }
}