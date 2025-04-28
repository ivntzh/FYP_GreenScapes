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
    public RawImage submitUI;
    public RawImage doneUI;

    [Header("Planting Progress")]
    public GameObject shovelDirtChild;
    public GameObject potFlatDirtChild;
    public GameObject potMixedDirt;
    public XRSocketInteractor seedSocket;

    [Header("Watering Timer Detection")]
    public GameObject[] wateringTimerUIs;   // ✅ Array of watering timers

    [Header("Submit System")]
    public GameObject bluePot;
    public XRSocketInteractor submitSocket;

    private bool soilWasWet = false;
    private bool wateringPhaseFinished = false;
    private bool submitStarted = false;
    private bool doneTriggered = false;
    private bool dirtAttachedTriggered = false;
    private bool dirtDroppedTriggered = false;
    private bool soilMixedTriggered = false;
    private bool seedPlaced = false;

    void Start()
    {
        EnableOnly(shovelOutline, shovelUI);
    }

    void Update()
    {
        // 🌱 Seed placement detection
        if (!seedPlaced && seedSocket != null)
        {
            var selectedSeed = seedSocket.GetOldestInteractableSelected();
            if (selectedSeed != null && selectedSeed.transform.CompareTag("Seed"))
            {
                seedPlaced = true;
                Debug.Log("🌱 Seed placed — Show watering can!");
                EnableOnly(wateringCanOutline, wateringCanUI); // ✅ directly show watering can
            }
        }

        // 💧 Watering timer UI detection
        bool anyTimerActive = false;
        foreach (var timer in wateringTimerUIs)
        {
            if (timer != null && timer.activeInHierarchy)
            {
                anyTimerActive = true;
                break;
            }
        }

        if (anyTimerActive)
        {
            if (!soilWasWet)
            {
                soilWasWet = true;
                Debug.Log("🌧️ Water detected — Showing potUI_Timer!");
                EnableOnly(null, potUI_Timer);
            }
        }
        else
        {
            if (soilWasWet && !wateringPhaseFinished)
            {
                wateringPhaseFinished = true;
                Debug.Log("🏜️ Watering finished — Showing potUI_WateringStage!");
                EnableOnly(potOutline, potUI_WateringStage); // ✅ Show watering stage
            }
        }

        // 📦 Blue pot destroyed detection
        if (!submitStarted && bluePot == null)
        {
            submitStarted = true;
            Debug.Log("📦 Blue pot destroyed — Show submit UI!");
            EnableOnly(null, submitUI);
        }

        // 🛒 Submitted plant detection
        if (submitStarted && !doneTriggered && submitSocket != null && submitSocket.hasSelection)
        {
            doneTriggered = true;
            var selected = submitSocket.GetOldestInteractableSelected();
            if (selected != null) Destroy(selected.transform.gameObject);
            Debug.Log("✅ Submitted — Show done UI!");
            EnableOnly(null, doneUI);
            Invoke(nameof(RestartTutorial), 5f);
        }

        // 🛠️ Dirt attached to shovel detection
        if (shovelDirtChild != null && shovelDirtChild.activeSelf && !dirtAttachedTriggered)
        {
            dirtAttachedTriggered = true;
            Debug.Log("🛠️ Dirt attached to shovel!");
            OnDirtAttachedToShovel();
        }

        // 🪣 Dirt dropped into pot detection
        if (potFlatDirtChild != null && potFlatDirtChild.activeSelf && !dirtDroppedTriggered)
        {
            dirtDroppedTriggered = true;
            Debug.Log("🪣 Dirt dropped into pot!");
            OnDirtDroppedInPot();
        }

        // 🌱 Soil mixed detection
        if (!soilMixedTriggered && potFlatDirtChild != null && potMixedDirt != null)
        {
            if (!potFlatDirtChild.activeSelf && potMixedDirt.activeSelf)
            {
                soilMixedTriggered = true;
                Debug.Log("🌱 Soil mixed!");
                OnSoilMixed();
            }
        }
    }

    void RestartTutorial()
    {
        Debug.Log("🔄 Restarting tutorial...");
        ResetFlags();
        Start();
    }

    void ResetFlags()
    {
        soilWasWet = false;
        wateringPhaseFinished = false;
        submitStarted = false;
        doneTriggered = false;
        dirtAttachedTriggered = false;
        dirtDroppedTriggered = false;
        soilMixedTriggered = false;
        seedPlaced = false;
    }

    public void OnShovelGrabbed() => EnableOnly(dirtPlateOutline, dirtPlateUI);
    public void OnDirtAttachedToShovel() => EnableOnly(potOutline, potUI_DirtDropped);
    public void OnDirtDroppedInPot() => EnableOnly(rakeOutline, rakeUI);
    public void OnRakeGrabbed() => EnableOnly(potOutline, potUI_MixedSoil);
    public void OnSoilMixed() => EnableOnly(seedOutline, seedUI);
    public void OnSeedGrabbed() => EnableOnly(potOutline, potUI_WithSeed);

    public void OnWateringCanGrabbed()
    {
        potOutline.enabled = true;

        if (potUI_DirtDropped != null) potUI_DirtDropped.enabled = false;
        if (potUI_MixedSoil != null) potUI_MixedSoil.enabled = false;
        if (potUI_WithSeed != null) potUI_WithSeed.enabled = false;
        if (potUI_WateringStage != null) potUI_WateringStage.enabled = true;
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

        // Enable the selected
        if (targetOutline != null) targetOutline.enabled = true;
        if (targetUI != null) targetUI.enabled = true;
    }
}
