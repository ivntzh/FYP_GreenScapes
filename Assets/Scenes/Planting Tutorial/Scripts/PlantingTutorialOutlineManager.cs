using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlantingTutorialOutlineManager : MonoBehaviour
{
    public Outline shovelOutline;
    public Outline dirtPlateOutline;
    public Outline potOutline;
    public Outline rakeOutline;
    public Outline seedOutline;
    public Outline wateringCanOutline;

    public GameObject shovelDirtChild;
    public GameObject potFlatDirtChild;
    public GameObject potMixedDirt;
    public XRSocketInteractor seedSocket;

    private bool seedPlaced = false;

    void Start()
    {
        EnableOnly(shovelOutline);
    }

    public void OnShovelGrabbed()
    {
        EnableOnly(dirtPlateOutline);
    }

    public void OnDirtAttachedToShovel()
    {
        EnableOnly(potOutline);
    }

    public void OnDirtDroppedInPot()
    {
        EnableOnly(rakeOutline);
    }

    public void OnRakeGrabbed()
    {
        EnableOnly(potOutline);
    }

    public void OnSoilMixed()
    {
        EnableOnly(seedOutline);
    }

    public void OnSeedGrabbed()
    {
        EnableOnly(potOutline);
    }

    void Update()
    {
        if (!seedPlaced && seedSocket.hasSelection)
        {
            seedPlaced = true;
            EnableOnly(wateringCanOutline);
        }
    }

    public void OnWateringCanGrabbed()
    {
        potOutline.enabled = true;
    }

    void EnableOnly(Outline target)
    {
        shovelOutline.enabled = false;
        dirtPlateOutline.enabled = false;
        potOutline.enabled = false;
        rakeOutline.enabled = false;
        seedOutline.enabled = false;
        wateringCanOutline.enabled = false;

        if (target != null)
            target.enabled = true;
    }
}