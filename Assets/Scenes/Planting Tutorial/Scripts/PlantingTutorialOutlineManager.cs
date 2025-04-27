using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlantingTutorialOutlineManager : MonoBehaviour
{
    [Header("Outlines")]
    public Outline shovelOutline;
    public Outline dirtPlateOutline;
    public Outline potOutline;
    public Outline rakeOutline;
    public Outline seedOutline;
    public Outline wateringCanOutline;

    [Header("Tutorial UI Images")]
    public RawImage shovelUI;
    public RawImage dirtPlateUI;
    public RawImage rakeUI;
    public RawImage seedUI;
    public RawImage wateringCanUI;

    [Header("Pot UI Variants")]
    public RawImage potUI_DirtDropped;
    public RawImage potUI_MixedSoil;
    public RawImage potUI_WithSeed;
    public RawImage potUI_Timer;
    public RawImage potUI_WateringStage;

    [Header("New Submit/Done UI")]
    public RawImage submitUI;     // ⬅️ New UI after blue pot destroyed
    public RawImage doneUI;       // ⬅️ New UI after submit success

    [Header("Planting Progress")]
    public GameObject shovelDirtChild;
    public GameObject potFlatDirtChild;
    public GameObject potMixedDirt;
    public XRSocketInteractor seedSocket;

    [Header("Material Monitoring")]
    public MeshRenderer soilRenderer;
    public Material drySoilMaterial;
    public GameObject soil;

    [Header("Submit System")]
    public GameObject bluePot;                // ⬅️ Your blue pot object
    public XRSocketInteractor submitSocket;   // ⬅️ Submit area socket

    private bool soilWasWet = false;
    private bool seedPlaced = false;
    private bool wateringPhaseFinished = false;
    private bool submitStarted = false;
    private bool doneTriggered = false;

    void Start()
    {
        EnableOnly(shovelOutline, shovelUI);
    }

    void Update()
    {
        if (!seedPlaced && seedSocket.hasSelection)
        {
            seedPlaced = true;
            EnableOnly(wateringCanOutline, wateringCanUI);
        }

        if (soilRenderer != null && soil != null && soil.activeInHierarchy)
        {
            if (!soilWasWet && soilRenderer.material != drySoilMaterial)
            {
                soilWasWet = true;
                Debug.Log("🌧️ Soil became wet — Showing potUI_Timer");
                EnableOnly(null, potUI_Timer);
            }
            else if (soilWasWet && soilRenderer.material == drySoilMaterial && !wateringPhaseFinished)
            {
                wateringPhaseFinished = true;
                Debug.Log("🏜️ Soil became dry again — Showing wateringCanUI");
                EnableOnly(wateringCanOutline, wateringCanUI);
            }
        }

        // 💥 New: Check if BluePot destroyed
        if (!submitStarted && bluePot == null)
        {
            submitStarted = true;
            Debug.Log("💥 Blue pot destroyed — Showing submit UI!");
            EnableOnly(null, submitUI);
        }

        // 🛒 New: Check if something submitted
        if (submitStarted && !doneTriggered && submitSocket != null && submitSocket.hasSelection)
        {
            doneTriggered = true;

            // Destroy the submitted object
            var selected = submitSocket.GetOldestInteractableSelected();
            if (selected != null)
            {
                Destroy(selected.transform.gameObject);
            }

            Debug.Log("✅ Submitted successfully — Showing done UI!");
            EnableOnly(null, doneUI);

            // ✅ After 5 seconds, call Start() again
            Invoke(nameof(RestartTutorial), 5f);
        }
    }
    void RestartTutorial()
    {
        Debug.Log("🔄 Restarting tutorial...");

        Start(); // Call Start() again to reset flow
    }

    public void OnShovelGrabbed()
    {
        EnableOnly(dirtPlateOutline, dirtPlateUI);
    }

    public void OnDirtAttachedToShovel()
    {
        EnableOnly(potOutline, potUI_DirtDropped);
    }

    public void OnDirtDroppedInPot()
    {
        EnableOnly(rakeOutline, rakeUI);
    }

    public void OnRakeGrabbed()
    {
        EnableOnly(potOutline, potUI_MixedSoil);
    }

    public void OnSoilMixed()
    {
        EnableOnly(seedOutline, seedUI);
    }

    public void OnSeedGrabbed()
    {
        EnableOnly(potOutline, potUI_WithSeed);
    }

    public void OnWateringCanGrabbed()
    {
        potOutline.enabled = true;

        if (potUI_DirtDropped != null) potUI_DirtDropped.enabled = false;
        if (potUI_MixedSoil != null) potUI_MixedSoil.enabled = false;
        if (potUI_WithSeed != null) potUI_WithSeed.enabled = false;

        if (potUI_WateringStage != null)
            potUI_WateringStage.enabled = true;
    }

    void EnableOnly(Outline targetOutline, RawImage targetUI)
    {
        // Disable all outlines
        shovelOutline.enabled = false;
        dirtPlateOutline.enabled = false;
        potOutline.enabled = false;
        rakeOutline.enabled = false;
        seedOutline.enabled = false;
        wateringCanOutline.enabled = false;

        // Disable all UIs
        if (shovelUI != null) shovelUI.enabled = false;
        if (dirtPlateUI != null) dirtPlateUI.enabled = false;
        if (potUI_DirtDropped != null) potUI_DirtDropped.enabled = false;
        if (potUI_MixedSoil != null) potUI_MixedSoil.enabled = false;
        if (potUI_WithSeed != null) potUI_WithSeed.enabled = false;
        if (potUI_Timer != null) potUI_Timer.enabled = false;
        if (potUI_WateringStage != null) potUI_WateringStage.enabled = false;
        if (rakeUI != null) rakeUI.enabled = false;
        if (seedUI != null) seedUI.enabled = false;
        if (wateringCanUI != null) wateringCanUI.enabled = false;
        if (submitUI != null) submitUI.enabled = false;
        if (doneUI != null) doneUI.enabled = false;

        // Enable the selected one
        if (targetOutline != null)
            targetOutline.enabled = true;
        if (targetUI != null)
            targetUI.enabled = true;
    }
}
