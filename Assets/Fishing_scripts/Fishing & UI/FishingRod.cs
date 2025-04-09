using UnityEngine;

public class FishingRod : MonoBehaviour
{
    public Transform bait; // Reference to the bait object
    public float castSpeed = 1.0f; // Speed of the casting
    public bool isBaitInArea = false; // Whether bait is inside fishing area
    private bool isGrabbed = false;
    private FishingGameManager gameManager;

    private void Start()
    {
        gameManager = FindAnyObjectByType<FishingGameManager>();
    }

    private void Update()
    {
        // Optionally, use this to adjust rod behavior (e.g., swinging, repositioning)
        // Detect bait position and check if it's inside the fishing area.
        // No need for user input to cast the bait manually anymore.
    }

    // physically picks up the rod in VR:
    public void OnGrab()
    {
        if (!isGrabbed)
        {
            isGrabbed = true;
            if (gameManager != null)
            {
                gameManager.OnRodGrabbed();
            }
        }
    }

    // If user releases the rod
    public void OnRelease()
    {
        isGrabbed = false;
    }

    // This function could be called when bait enters the fishing area
    public void TriggerFishing()
    {
        // Logic for fishing when bait is in the area
        // You can randomly determine whether a fish or rubbish is caught
        Debug.Log("Fishing triggered...");

        // Example of triggering the catch
        StartFishing();
    }

    private void StartFishing()
    {
        // Add your fishing logic, such as randomizing the catch (fish or rubbish)
        int randomCatch = Random.Range(0, 2); // 0 = Fish, 1 = Rubbish

        if (randomCatch == 0)
        {
            Debug.Log("Caught a Fish!");
            // Call any fish-related functionality here (add to inventory, update score, etc.)
        }
        else
        {
            Debug.Log("Caught Rubbish!");
            // Call rubbish-related functionality here (manage waste, etc.)
        }
    }
}
