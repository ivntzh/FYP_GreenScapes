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
    public RawImage potUI_WateringStage;

    [Header("Planting Progress")]
    public GameObject shovelDirtChild;
    public GameObject potFlatDirtChild;
    public GameObject potMixedDirt;
    public XRSocketInteractor seedSocket;

    private bool seedPlaced = false;

    void Start()
    {
        EnableOnly(shovelOutline, shovelUI);
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

    void Update()
    {
        if (!seedPlaced && seedSocket.hasSelection)
        {
            seedPlaced = true;
            EnableOnly(wateringCanOutline, wateringCanUI);
        }
    }

    public void OnWateringCanGrabbed()
    {
        potOutline.enabled = true;

        // Disable all pot UIs first (safe)
        if (potUI_DirtDropped != null) potUI_DirtDropped.enabled = false;
        if (potUI_MixedSoil != null) potUI_MixedSoil.enabled = false;
        if (potUI_WithSeed != null) potUI_WithSeed.enabled = false;

        // ✅ Show the watering pot UI instead
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
        if (rakeUI != null) rakeUI.enabled = false;
        if (seedUI != null) seedUI.enabled = false;
        if (wateringCanUI != null) wateringCanUI.enabled = false;

        // Enable the one you want
        if (targetOutline != null)
            targetOutline.enabled = true;
        if (targetUI != null)
            targetUI.enabled = true;
    }
}