using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using TMPro;

public class FishingArea : MonoBehaviour
{
    private enum FishState { Idle, Waiting, Prompt, Pulling, Caught }
    private FishState state = FishState.Idle;
    // at top of FishingArea
    private bool waitingForRubbish = false;


    [Header("References")]
    public GameObject       bait;               // Your bait prefab instance
    public GameObject       rod;
    public FishingString    fishingString;      // The rope simulator
    public Transform        startPoint;         // Should match fishingString.startPoint
    public FishingGameManager gameManager;      // Assign or Find in Start()
    public Transform        objectTP;     
    public Transform        rodTP;

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

    [Header("Rubbish Object Settings")]
    public AudioClip        disposalSfx;            // drag your “bin drop” sound here
    // public GameObject       disposalPopupPanel;     // a small panel with text, initially inactive
    // public TextMeshProUGUI  disposalPopupText;      // the “+90 Coins!” text
    // public float            disposalPopupDuration = 2f;  // seconds to show the popup


    [Header("Fish Objects")]
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
        if (gameManager.currentState == GameState.GameOver) return;
        state = FishState.Waiting;
        catchTime    = Random.Range(5f, 10f);
        fishingTimer = Time.time + catchTime;
        Debug.Log($"[Fishing] Timer started: {catchTime}s");
    }

    private void Update()
    {
        if (gameManager.currentState == GameState.GameOver)
        {
            // send a zero‐strength pulse to stop any ongoing vibration
            HapticManager.Instance.PulseBoth(0f, 0f);

            catchPromptPanel.SetActive(false);
            catchResultPanel.SetActive(false);
            exclamationIcon?.SetActive(false);

            ResetFishingArea();
            
            rod.transform.position = rodTP.position;
            rod.transform.rotation = rodTP.rotation;

            if (currentModel != null )
            {
                // detach from bait (optional)
                currentModel.transform.SetParent(null, true);

                // snap to objectTP’s world position & rotation
                currentModel.transform.position = objectTP.position;
                currentModel.transform.rotation = objectTP.rotation;
            }
        }

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
        if (gameManager.currentState == GameState.GameOver) return;
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

        // only pulse if we’re still actually playing
        if (gameManager.currentState == GameState.Playing)
        {
            HapticManager.Instance.PulseBoth(t, 0.02f);
        }

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
        Invoke(nameof(ResetFishingArea), 0.5f);
        Invoke(nameof(ResetFishGameUI), 5f);
    }


    private void CancelFishing()
    {
        Debug.Log("[Fishing] Canceled.");
        ResetFishingArea();
    }

    private void ShowCatchResult()
    {
        // Disable exclamation prompt
        exclamationIcon?.SetActive(false);
        catchPromptPanel.SetActive(false);
        catchResultPanel.SetActive(true);


        if (bait == null)
        {
            Debug.LogError("Bait object is not assigned!");
            return;
        }

        // Randomize the result
        float randomValue = Random.value;
        int   pointsAwarded = 0;
        GameObject modelToSpawn = null;

        if (randomValue < 0.6f)
        {
            // Fish
            catchResultText.text = "You caught a fish!";
            modelToSpawn   = fishModel;
            pointsAwarded  = 100;
            PlaySound(fishCatchSound);
        }
        else if (randomValue < 0.9f)
        {
            // Rubbish
            catchResultText.text = "You caught some rubbish!\nThrow it in the bin!";
            modelToSpawn   = rubbishModel;
            pointsAwarded  = 25;             // immediate rubbish points
            PlaySound(rubbishCatchSound);
            waitingForRubbish = true;        // hold reset until disposal

        }
        else
        {
            // Treasure
            catchResultText.text = "You found a treasure!";
            modelToSpawn   = treasureModel;
            pointsAwarded  = 200;
            PlaySound(treasureCatchSound);
        }

        // Award fish, treasure, or the 10pts for rubbish
        if (gameManager.currentState == GameState.Playing)
            gameManager.AddScore(pointsAwarded);

        if (modelToSpawn == null)
        {
            Debug.LogError("modelToSpawn is null!");
            return;
        }

        // Spawn it as a child of the bait (so it follows during pull)
        currentModel = Instantiate(modelToSpawn, bait.transform.position, Quaternion.identity);
        currentModel.transform.SetParent(bait.transform);
        currentModel.transform.localPosition = Vector3.zero;

        // Fish & treasure auto‑destroy after 5s
        if (!waitingForRubbish)
        {
            Destroy(currentModel, 5f);  // destroys after 5 seconds :contentReference[oaicite:0]{index=0}
        }

        state = FishState.Caught;
    }

    public void OnRubbishDisposed()
    {
        if (!waitingForRubbish) return;

        waitingForRubbish = false;

        // award the final 90 points
        if (gameManager.currentState == GameState.Playing)
            gameManager.AddScore(125);

        PlaySound(disposalSfx);

        // clean up the model
        if (currentModel != null)
            Destroy(currentModel);

        // reset for next cast
        ResetFishingArea();
    }


    private void ResetFishingArea()
    {
        state = FishState.Idle;

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

    private void ResetFishGameUI()
    {
        catchPromptPanel.SetActive(false);
        catchResultPanel.SetActive(false);
        exclamationIcon?.SetActive(false);
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
