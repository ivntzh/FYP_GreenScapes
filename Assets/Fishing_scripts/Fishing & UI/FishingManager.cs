using UnityEngine;

public class FishingManager : MonoBehaviour
{
    public FishingArea fishingArea;  // Reference to the fishing area
    public FishingRod fishingRod;   // Reference to the fishing rod

    private void Update()
    {
        // if (fishingArea.CanFish() && Input.GetKeyDown(KeyCode.Space))
        // {
        //     fishingRod.CastLine();
        // }

        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     fishingRod.ReelIn();
        // }
    }
}
