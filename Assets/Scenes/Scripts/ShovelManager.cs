using UnityEngine;
using Photon.Pun;

public class ShovelManager : MonoBehaviourPun
{
    [Header("Visuals")]
    public GameObject dirtOnShovel;          // the little clump on your shovel
    public GameObject droppedDirtPrefab;     // must be in Resources/PhotonPrefabs
    public ParticleSystem dirtPickupParticles;
    public AudioSource pickupSound;

    bool hasDirt = false;

    void Start()
    {
        if (dirtOnShovel != null)
            dirtOnShovel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // only run once, and only when hitting the pile
        if (!hasDirt && other.CompareTag("DirtPile"))
        {
            // broadcast to everyone that we just picked up dirt
            photonView.RPC(nameof(RPC_PickupDirt), RpcTarget.AllBuffered);
        }
    }

    // Called (locally) by your XR interaction when the player uses the shovel
    public void OnActivate()
    {
        if (!hasDirt || droppedDirtPrefab == null) return;

        // broadcast to everyone that we¡¯re dropping dirt at this spot
        photonView.RPC(
            nameof(RPC_DropDirt),
            RpcTarget.AllBuffered,
            dirtOnShovel.transform.position,
            dirtOnShovel.transform.rotation
        );
    }

    [PunRPC]
    void RPC_PickupDirt()
    {
        // All clients do the same visuals
        hasDirt = true;
        if (dirtOnShovel != null) dirtOnShovel.SetActive(true);
        if (dirtPickupParticles != null) dirtPickupParticles.Play();
        if (pickupSound != null) pickupSound.Play();
    }

    [PunRPC]
    void RPC_DropDirt(Vector3 pos, Quaternion rot)
    {
        // MasterClient *or* everyone can do this once via buffered RPC
        PhotonNetwork.Instantiate(droppedDirtPrefab.name, pos, rot);

        // Clear the shovel visually on all clients
        hasDirt = false;
        if (dirtOnShovel != null) dirtOnShovel.SetActive(false);
    }
}
