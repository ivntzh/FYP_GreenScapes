using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class SoilGrowthOnParticle : MonoBehaviour
{
    [Header("Growth Settings")]
    public GameObject smallPlant;
    public GameObject mediumPlant;

    [Header("Soil Visuals")]
    public Material wetSoilMaterial;
    public Material drySoilMaterial;
    public MeshRenderer soilRenderer;

    [Header("Seed Setup")]
    public XRSocketInteractor seedSocket;
    public string seedTag = "Seed";

    [Header("Timers")]
    public float waterTimeRequired = 2f;
    public float timeToSmallPlant = 5f;
    public float timeToMediumPlant = 7f;

    private float waterTimer = 0f;
    private bool isWatering = false;
    private bool hasSeedGrown = false;
    private bool isSoilWet = false;
    private bool isReadyForStage2 = false;
    private bool isStage2Watering = false;

    void Update()
    {
        if (isWatering)
        {
            waterTimer += Time.deltaTime;

            if (waterTimer >= waterTimeRequired)
            {
                isWatering = false;
                waterTimer = 0f;

                if (!hasSeedGrown)
                {
                    // Stage 1: grow seed into small plant
                    WetSoil();
                    Invoke(nameof(GrowSmallPlant), timeToSmallPlant);
                }
                else if (isReadyForStage2 && !isStage2Watering)
                {
                    // Stage 2: grow small plant into medium plant
                    isStage2Watering = true;
                    WetSoil();
                    Invoke(nameof(GrowMediumPlant), timeToMediumPlant);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isWatering = true;
            Debug.Log("💧 Watering started");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isWatering = false;
            waterTimer = 0f;
            Debug.Log("💧 Watering stopped");
        }
    }

    private void WetSoil()
    {
        isSoilWet = true;
        if (soilRenderer != null && wetSoilMaterial != null)
            soilRenderer.material = wetSoilMaterial;

        Debug.Log("🌧️ Soil turned wet");
    }

    private void DrySoil()
    {
        isSoilWet = false;
        if (soilRenderer != null && drySoilMaterial != null)
            soilRenderer.material = drySoilMaterial;

        Debug.Log("🏜️ Soil turned dry");
    }

    private void GrowSmallPlant()
    {
        var seed = seedSocket.GetOldestInteractableSelected();

        if (seedSocket.hasSelection && seed != null && seed.transform.CompareTag(seedTag))
        {
            Destroy(seed.transform.gameObject);
            if (smallPlant != null) smallPlant.SetActive(true);
            Debug.Log("🌱 Seed grew into small plant");

            hasSeedGrown = true;
            isReadyForStage2 = true;
            DrySoil();
        }
        else
        {
            Debug.Log("❌ No seed in socket, skipping small plant growth.");
        }
    }

    private void GrowMediumPlant()
    {
        if (smallPlant != null) smallPlant.SetActive(false);
        if (mediumPlant != null) mediumPlant.SetActive(true);

        Debug.Log("🌿 Small plant grew into medium plant");
        DrySoil();
    }
}