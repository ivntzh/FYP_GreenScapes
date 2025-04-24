using Photon.Pun;
using UnityEngine;

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
        if (other.CompareTag("DirtPile") && !hasDirt)
        {
            dirtOnShovel.SetActive(true);
            hasDirt = true;

            if (dirtPickupParticles != null) dirtPickupParticles.Play();
            if (pickupSound != null) pickupSound.Play();

            Debug.Log("Shovel picked up dirt.");
        }
    }

    public void OnActivate()
    {
        if (!hasDirt || droppedDirtPrefab == null) return;

        PhotonNetwork.Instantiate(droppedDirtPrefab.name, dirtOnShovel.transform.position, dirtOnShovel.transform.rotation);

        dirtOnShovel.SetActive(false);
        hasDirt = false;
    }
}
