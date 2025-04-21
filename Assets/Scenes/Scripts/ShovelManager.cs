using UnityEngine;

public class ShovelManager : MonoBehaviour
{
    public GameObject dirtOnShovel;             // The small dirt on the shovel
    public GameObject droppedDirtPrefab;        // The prefab to spawn
    public ParticleSystem dirtPickupParticles;  // Particle FX to play when scooping
    public AudioSource pickupSound;             // Sound FX to play when scooping

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

            // Play particle effect
            if (dirtPickupParticles != null)
                dirtPickupParticles.Play();

            // Play sound effect
            if (pickupSound != null)
                pickupSound.Play();

            Debug.Log("Shovel picked up dirt.");
        }
    }

    public void OnActivate()
    {
        if (hasDirt && dirtOnShovel != null)
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
        }
    }
}