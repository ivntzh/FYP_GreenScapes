using UnityEngine;
using Photon.Pun;

public class ShovelManager : MonoBehaviour
{
    public GameObject dirtOnShovel;             // The small dirt on the shovel
    public GameObject droppedDirtPrefab;        // The prefab to spawn
    public ParticleSystem dirtPickupParticles;  // Particle FX to play when scooping
    public AudioSource pickupSound;             // Sound FX to play when scooping
    public PhotonView photonView;               // Reference to PhotonView

    private bool hasDirt = false;

    void Start()
    {
        if (dirtOnShovel != null)
            dirtOnShovel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DirtPile") && !hasDirt)
        {
            if (photonView.IsMine)
            {
                dirtOnShovel.SetActive(true);
                hasDirt = true;

                // Play particle effect
                if (dirtPickupParticles != null)
                    dirtPickupParticles.Play();

                // Play sound effect
                if (pickupSound != null)
                    pickupSound.Play();

                Debug.Log("Shovel picked up dirt.");

                // Notify other clients
                photonView.RPC("RPC_PickupDirt", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void RPC_PickupDirt()
    {
        dirtOnShovel.SetActive(true);
        hasDirt = true;
    }

    public void OnActivate()
    {
        if (hasDirt && dirtOnShovel != null)
        {
            if (photonView.IsMine)
            {
                if (droppedDirtPrefab != null)
                {
                    Instantiate(
                        droppedDirtPrefab,
                        dirtOnShovel.transform.position,
                        dirtOnShovel.transform.rotation
                    );
                }

                dirtOnShovel.SetActive(false);
                hasDirt = false;

                // Notify other clients
                photonView.RPC("RPC_DropDirt", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void RPC_DropDirt()
    {
        dirtOnShovel.SetActive(false);
        hasDirt = false;
    }
}