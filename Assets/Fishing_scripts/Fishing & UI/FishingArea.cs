using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using TMPro;

public class FishingArea : MonoBehaviour
{
    private enum FishState { Idle, Waiting, Prompt, Pulling, Caught }
    private FishState state = FishState.Idle;

    [Header("References")]
    public GameObject       bait;               // Your bait prefab instance
    public FishingString    fishingString;      // The rope simulator
    public Transform        startPoint;         // Should match fishingString.startPoint
    public FishingGameManager gameManager;      // Assign or Find in Start()

    [Header("UI & Audio")]
    public GameObject       catchPromptPanel;
    public TextMeshProUGUI  catchPromptText;
    public GameObject       catchResultPanel;
    public TextMeshProUGUI  catchResultText;
    public GameObject       exclamationIcon;
    public AudioClip        enterAreaSound;
    public AudioClip        leaveAreaDuringTimerSound;
    public AudioClip        catchPromptSound;
    public AudioClip        fishCatchSound;
    public AudioClip        rubbishCatchSound;
    public AudioClip        treasureCatchSound;
    private AudioSource     audioSource;

    public GameObject fishModel; // Model to spawn for fish
    public GameObject rubbishModel; // Model to spawn for rubbish
    public GameObject treasureModel; // Model to spawn for treasure

    [Header("Reel‑In Settings")]
    public float            minPullDistance = 0.1f;
    public float            maxPullDistance = 1.5f;
    public float            extraSlack      = 0.2f;

    [Header("Rope Extension Limits")]
    [Tooltip("The shortest the line will ever be (in meters)")]
    public float minRopeLength = 0.5f;

    [Tooltip("The longest the line can stretch (in meters)")]
    public float maxRopeLength = 2.0f;


    // Internal
    private float           fishingTimer;
    private float           catchTime;
    private GameObject      currentModel;


    void Start()
    {
        audioSource       = GetComponent<AudioSource>();
        catchPromptPanel.SetActive(false);
        catchResultPanel.SetActive(false);
        exclamationIcon?.SetActive(false);

        // auto‑find gameManager if you like:
        if (gameManager == null)
            gameManager = FindAnyObjectByType<FishingGameManager>();

        // auto‑assign startPoint from the rope
        if (startPoint == null && fishingString != null)
            startPoint = fishingString.startPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bait") && state == FishState.Idle)
        {
            Debug.Log("[Fishing] Entered pond → Idle→Waiting");
            StartFishingTimer();
            PlaySound(enterAreaSound);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Bait")) 
            return;

        Debug.Log($"[Fishing] Exit pond in state={state}");

        switch (state)
        {
            case FishState.Waiting:
                Debug.Log("[Fishing] Exit during Waiting → cancel");
                CancelFishing();
                PlaySound(leaveAreaDuringTimerSound);
                break;

            case FishState.Prompt:
                Debug.Log("[Fishing] Prompt→Pulling");
                FreezeBait();
                if (fishingString != null)
                    fishingString.clampEndPoint = false;
                state = FishState.Pulling;
                catchPromptPanel.SetActive(false);
                exclamationIcon?.SetActive(false);
                break;

            // we explicitly ignore these:
            case FishState.Idle:
            case FishState.Pulling:
            case FishState.Caught:
                // do nothing
                break;
        }
    }


    private void StartFishingTimer()
    {
        state = FishState.Waiting;
        catchTime    = Random.Range(5f, 10f);
        fishingTimer = Time.time + catchTime;
        Debug.Log($"[Fishing] Timer started: {catchTime}s");
    }

    private void Update()
    {
        // 1) waiting for a bite
        if (state == FishState.Waiting && Time.time >= fishingTimer)
            CompleteFishingTimer();

        // 2) reeling in
        if (state == FishState.Pulling)
            HandlePulling();
    }

    private void CompleteFishingTimer()
    {
        Debug.Log($"[Fishing] CompleteTimer called in state={state}");
        if (state != FishState.Waiting) return;

        state = FishState.Prompt;
        Debug.Log("[Fishing] Waiting→Prompt (bite!)");

        catchPromptPanel.SetActive(true);
        catchPromptText.text = "Something's on the line! Pull to reel in!";
        exclamationIcon?.SetActive(true);
        PlaySound(catchPromptSound);

        // initial haptic “bite” cue
        HapticManager.Instance.PulseBoth(0.8f, 0.3f);
    }

    private void HandlePulling()
    {
        float pullDist = Vector3.Distance(startPoint.position, bait.transform.position);
        Debug.Log($"[Fishing] Pulling: distance={pullDist:F2}");
        
        float t = Mathf.InverseLerp(minPullDistance, maxPullDistance, pullDist);
        Debug.Log($"[Fishing] Pulling: normalized t={t:F2}");

        if (fishingString != null)
        {
            // desired length based on pull distance + slack
            float desiredLength = pullDist + extraSlack;

            // clamp it between your min and max
            float clampedLength = Mathf.Clamp(desiredLength, minRopeLength, maxRopeLength);

            // apply it to the rope
            fishingString.maxRopeLength = clampedLength;

            Debug.Log($"[Fishing] Rope length set to {fishingString.maxRopeLength:F2}");
        }
        else
        {
            Debug.LogWarning("[Fishing] HandlePulling: fishingString is null!");
        }

        HapticManager.Instance.PulseBoth(t, 0.02f);

        if (t >= 1f)
            OnPullSuccess();
    }


    private void OnPullSuccess()
    {
        state = FishState.Caught;

        // 1) Immediately unfreeze the bait so it can fall/reset
        UnfreezeBait();

        // 2) Re‑enable the rope clamp so future casts behave normally
        if (fishingString != null)
            fishingString.clampEndPoint = true;

        // 3) Award points & show result UI
        ShowCatchResult();

        // 4) Finally, do the full reset (rope length, state, UI) after a short delay
        Invoke(nameof(ResetFishingArea), 1f);
    }


    private void CancelFishing()
    {
        Debug.Log("[Fishing] Canceled.");
        ResetFishingArea();
    }

    private void ShowCatchResult()
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
        Destroy(currentModel, 5f);

    }

    private void ResetFishingArea()
    {
        state = FishState.Idle;

        catchPromptPanel.SetActive(false);
        catchResultPanel.SetActive(false);
        exclamationIcon?.SetActive(false);

        // unfreeze bait
        if (bait.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity  = true;
        }

        // reset rope
        if (fishingString != null)
        {
            fishingString.clampEndPoint = true;
            fishingString.maxRopeLength = fishingString.segmentLength * fishingString.segmentCount;
        }

    }


    private void FreezeBait()
    {
        if (bait.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity  = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;  // freeze position & rotation
            Debug.Log("[Fishing] Bait frozen.");
        }
    }

    private void UnfreezeBait()
    {
        if (bait.TryGetComponent<Rigidbody>(out var rb))
        {
            // Remove all constraints so it can move/rotate normally
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity  = true;
            Debug.Log("[Fishing] Bait unfrozen.");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
