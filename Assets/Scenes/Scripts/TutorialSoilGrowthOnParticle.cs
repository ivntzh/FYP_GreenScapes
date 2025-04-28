using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class TutorialSoilGrowthOnParticle : MonoBehaviour
{
    [Header("Growth Prefabs")]
    public GameObject smallPlant;
    public GameObject mediumPlant;
    public GameObject finalFullPlantPrefab;      // Final full-grown object with pot

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
    public float timeToFinalReplace = 10f;

    [Header("Growth Root")]
    public GameObject plantParent; // The parent object that will be destroyed

    private float waterTimer = 0f;
    private bool isWatering = false;
    private bool hasSeedGrown = false;
    private bool isSoilWet = false;
    private bool isReadyForStage2 = false;
    private bool isStage2Watering = false;
    private bool isReadyForFinalStage = false;
    private bool isFinalStageWatering = false;

    [Header("UI Panels")]
    public GameObject waterUI;
    public GameObject timer1UI;
    public GameObject timer2UI;
    public GameObject timer3UI;

    private bool hasShownWaterUI = false;

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
                    WetSoil();

                    // ✅ Show water UI (first time only)
                    if (!hasShownWaterUI && waterUI != null)
                    {
                        waterUI.SetActive(true);
                        hasShownWaterUI = true;
                    }

                    // ✅ Disable water UI and show first timer
                    if (waterUI != null) waterUI.SetActive(false);
                    if (timer1UI != null) timer1UI.SetActive(true);

                    // ✅ Delay grow
                    Invoke(nameof(GrowSmallPlant), timeToSmallPlant);
                }
                else if (isReadyForStage2 && !isStage2Watering)
                {
                    isStage2Watering = true;
                    WetSoil();

                    // ✅ Disable water UI and show second timer
                    if (waterUI != null) waterUI.SetActive(false);
                    if (timer1UI != null) timer1UI.SetActive(false);
                    if (timer2UI != null) timer2UI.SetActive(true);

                    Invoke(nameof(GrowMediumPlant), timeToMediumPlant);
                }
                else if (isReadyForFinalStage && !isFinalStageWatering)
                {
                    isFinalStageWatering = true;
                    WetSoil();

                    // ✅ Disable all previous UIs and show final timer
                    if (waterUI != null) waterUI.SetActive(false);
                    if (timer2UI != null) timer2UI.SetActive(false);
                    if (timer3UI != null) timer3UI.SetActive(true);

                    Invoke(nameof(ReplaceWithFinalPlant), timeToFinalReplace);
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

        isReadyForFinalStage = true;
        DrySoil();

        Debug.Log("🌿 Small plant grew into medium plant");
    }

    private void ReplaceWithFinalPlant()
    {
        Debug.Log("🌳 Replacing with fully grown plant + pot");

        if (finalFullPlantPrefab != null && plantParent != null)
        {
            // Spawn new full plant
            Instantiate(finalFullPlantPrefab, plantParent.transform.position, plantParent.transform.rotation);

            // Destroy current plant + soil parent
            Destroy(plantParent);
        }
    }
}