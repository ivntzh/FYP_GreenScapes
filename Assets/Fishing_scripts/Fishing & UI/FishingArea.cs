using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingArea : MonoBehaviour
{
    public GameObject bait; // Reference to the bait GameObject
    
    public GameObject catchPromptPanel;
    public TextMeshProUGUI catchPromptText;
    public GameObject catchResultPanel;
    public TextMeshProUGUI catchResultText;
    public AudioClip enterAreaSound;
    public AudioClip leaveAreaDuringTimerSound;
    public AudioClip catchPromptSound;
    public AudioClip fishCatchSound;
    public AudioClip rubbishCatchSound;
    public AudioClip treasureCatchSound;
    private AudioSource audioSource;

    public GameObject fishModel; // Model to spawn for fish
    public GameObject rubbishModel; // Model to spawn for rubbish
    public GameObject treasureModel; // Model to spawn for treasure

    private bool isBaitInside = false;
    private bool isCatchReady = false;
    private bool isTimerRunning = false;
    private float catchTime;
    private GameObject currentModel = null; // Store the current spawned model
    private float fishingTimer; // Use a timer variable to handle time correctly
    private FishingGameManager gameManager;
    private Rod_Haptics RodHaptics;

    [Header("Prompts")]
    public GameObject exclamationIcon;  // drag Image here


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        catchPromptPanel.SetActive(false);
        catchResultPanel.SetActive(false);

        // Attempt to find the FishingGameManager in the scene
        gameManager = FindAnyObjectByType<FishingGameManager>();

        if (bait == null)
        {
            bait = GameObject.Find("Bait"); 
        }

        // Disable prompt on launch
        if (exclamationIcon != null)
        {
            exclamationIcon.SetActive(false);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bait") && !isBaitInside)
        {
            Debug.Log("Bait entered fishing area.");
            isBaitInside = true;
            StartFishingTimer();
            PlaySound(enterAreaSound);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bait") && isBaitInside)
        {
            Debug.Log("Bait left fishing area.");
            if (isTimerRunning)
            {
                // Cancel the timer and stop fishing
                CancelFishing();
                PlaySound(leaveAreaDuringTimerSound);
            }
            else if (isCatchReady)
            {
                // If the bait leaves after the catch is ready, show the result
                ShowCatchResult();
            }
            else
            {
                // If neither, just reset the area
                ResetFishingArea();
            }
            isBaitInside = false;
        }
    }

    private void StartFishingTimer()
    {
        if (!isTimerRunning && !isCatchReady)
        {
            isTimerRunning = true;
            catchTime = Random.Range(5f, 10f); // Set random fishing time
            Debug.Log($"Fishing timer started for {catchTime} seconds.");
            fishingTimer = Time.time + catchTime; // Set the end time of the timer
        }
    }

    private void Update()
    {
        // Check if the timer has reached the time and if it's still running
        if (isTimerRunning && Time.time >= fishingTimer)
        {
            CompleteFishingTimer();
        }
    }

    private void CompleteFishingTimer()
    {
        if (isBaitInside)
    {
        isTimerRunning = false;
        isCatchReady = true;
        catchPromptPanel.SetActive(true);
        catchPromptText.text = "There's something on the line!";
        PlaySound(catchPromptSound);

        // Exclamation icon
        if (exclamationIcon != null)
            exclamationIcon.SetActive(true);
        }

        // Haptics
        var haptics = RodHaptics;
        if (haptics != null)
        {
            haptics.Pulse(0.8f, 0.3f);
        }
    }

    private void CancelFishing()
    {
        Debug.Log("Fishing canceled. Timer stopped.");
        isTimerRunning = false;
        isCatchReady = false;
        catchPromptPanel.SetActive(false);
        ResetFishingArea();  // Reset fishing area for a new attempt
    }

    private void ShowCatchResult()
    {
        if (isCatchReady)
        {
            // Disable exclamation prompt
            if (exclamationIcon != null)
            {
                exclamationIcon.SetActive(false);
            }

            Debug.Log("Showing catch result.");
            catchPromptPanel.SetActive(false);
            catchResultPanel.SetActive(true);

            // Check if bait is null
            if (bait == null)
            {
                Debug.LogError("Bait object is not assigned!");
                return; // Exit the method if bait is null
            }

            // Randomize the result: 60% chance of fish, 30% chance of rubbish, 10% chance of treasure
            float randomValue = Random.value;

            // Give points based on catch result
            int pointsAwarded = 0;

            GameObject modelToSpawn = null;
            if (randomValue < 0.6f)
            {
                catchResultText.text = "You caught a fish!";
                modelToSpawn = fishModel;
                pointsAwarded = 100; // fish
                PlaySound(fishCatchSound);
            }
            else if (randomValue < 0.9f)
            {
                catchResultText.text = "You caught some rubbish!";
                modelToSpawn = rubbishModel;
                pointsAwarded = 50; // rubbish
                PlaySound(rubbishCatchSound);
            }
            else
            {
                catchResultText.text = "You found a treasure!";
                modelToSpawn = treasureModel;
                pointsAwarded = 200; // treasure
                PlaySound(treasureCatchSound);
            }

            // Only add score if the game is in the Playing state
            if (gameManager != null && gameManager.currentState == GameState.Playing)
            {
                gameManager.AddScore(pointsAwarded);
            }


            // Check if modelToSpawn is null
            if (modelToSpawn == null)
            {
                Debug.LogError("modelToSpawn is null! This should not happen.");
                return; // Exit the method if modelToSpawn is null
            }

            // Instantiate the model at the bait's position
            Debug.Log("Instantiating model at position: " + bait.transform.position);
            currentModel = Instantiate(modelToSpawn, bait.transform.position, Quaternion.identity); // Use bait's position

            // Attach the model to the bait
            currentModel.transform.SetParent(bait.transform); // Attach to bait object

            // Optionally adjust the model's position relative to the bait
            currentModel.transform.localPosition = Vector3.zero; // This can be adjusted if needed

            // Destroy the spawned model after 5 seconds
            Destroy(currentModel, 2.5f);
        }
        else
        {
            Debug.Log("No catch result to show.");
        }

        isCatchReady = false;
    }


    private void ResetFishingArea()
    {
        // This ensures that everything is reset properly
        catchPromptPanel.SetActive(false);
        catchResultPanel.SetActive(false);
        DestroyCurrentModel();  // Destroy any model that might be active
        isTimerRunning = false;
        isCatchReady = false;
    }

    private void DestroyCurrentModel()
    {
        if (currentModel != null)
        {
            Destroy(currentModel); // Destroy the current model if it's active
            currentModel = null; // Reset the reference
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
