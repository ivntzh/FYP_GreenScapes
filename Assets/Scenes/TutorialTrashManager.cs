using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR;

public class TutorialTrashManager : MonoBehaviour
{
    [Header("XR Components")]
    public Transform XROrigin;
    public Camera XRCamera;
    public CharacterController characterController;

    [Header("Settings")]
    public float fallThreshold = -5f;
    public Vector3 respawnPosition = new Vector3(44.5f, 1.6f, -5.4f);
    public float respawnDelay = 1f;

    [Header("Settings")]
    public float tofallThreshold = -5f;
    public Vector3 totutorialPosition = new Vector3(-400f, 1.6f, -418f);
    public float torespawnDelay = 1f;

    bool isRespawning = false;

    [System.Serializable]
    public class TrashData
    {
        public GameObject prefab;         // Trash prefab to instantiate
        public Vector3 spawnPosition;      // Where to spawn it
    }

    public static TutorialTrashManager Instance;

    public List<TrashData> allTrashList = new List<TrashData>(); // Fill this manually in Inspector

    private int totalTrashDestroyed = 0;
    private int totalTrash;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SpawnAllTrash(); // Spawn all trash at game start
    }

    private void SpawnAllTrash()
    {
        foreach (TrashData data in allTrashList)
        {
            Instantiate(data.prefab, data.spawnPosition, Quaternion.identity);
        }

        totalTrash = allTrashList.Count;
        Debug.Log($"Spawned {totalTrash} trash at start.");
    }

    public void TrashDestroyed()
    {
        totalTrashDestroyed++;

        if (totalTrashDestroyed >= totalTrash)
        {
            Debug.Log("All trash destroyed! Respawning after 5 seconds...");
            StartCoroutine(RespawnAllTrashAfterDelay(5f));
        }
    }

    private IEnumerator RespawnAllTrashAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnAllTrash();
        StartCoroutine(RespawnRoutine());
        totalTrashDestroyed = 0; // Reset counter
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // 1) disable locomotion/physics
        var bodyTransformer = GetComponent<XRBodyTransformer>();
        if (bodyTransformer != null) bodyTransformer.enabled = false;
        if (characterController != null) characterController.enabled = false;

        // 2) zero out camera and move XR Origin
        InputTracking.disablePositionalTracking = true;
        XRCamera.transform.localPosition = Vector3.zero;
        XRCamera.transform.localRotation = Quaternion.identity;
        XROrigin.position = respawnPosition;

        // 3) wait a frame (and physics step) to settle
        yield return new WaitForFixedUpdate();
        yield return null;

        // 4) re-enable physics/locomotion
        if (characterController != null) characterController.enabled = true;
        if (bodyTransformer != null) bodyTransformer.enabled = true;
        InputTracking.disablePositionalTracking = false;

        // 5) clear any residual velocity
        if (characterController != null)
            characterController.Move(Vector3.zero);

        // optional delay if you want a “teleport fade?effect
        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        isRespawning = false;
    }

    public void RespawnTrashImmediately(TutorialTrashType trash)
    {
        foreach (TrashData data in allTrashList)
        {
            // Match based on prefab name
            if (trash.name.Contains(data.prefab.name)) // match by prefab name
            {
                Instantiate(data.prefab, data.spawnPosition, Quaternion.identity);
                Debug.Log($"Respawned {data.prefab.name} because it was placed in the wrong bin.");
                return;
            }
        }

        Debug.LogWarning("No matching prefab found to respawn wrong trash!");
    }

    public void backtoMain()
    {
        StartCoroutine(RespawnRoutine());
    }

    public void gotoTutorial()
    {
        StartCoroutine(gototutorialRoutine());
    }

    private IEnumerator gototutorialRoutine()
    {
        isRespawning = true;

        // 1) disable locomotion/physics
        var bodyTransformer = GetComponent<XRBodyTransformer>();
        if (bodyTransformer != null) bodyTransformer.enabled = false;
        if (characterController != null) characterController.enabled = false;

        // 2) zero out camera and move XR Origin
        InputTracking.disablePositionalTracking = true;
        XRCamera.transform.localPosition = Vector3.zero;
        XRCamera.transform.localRotation = Quaternion.identity;
        XROrigin.position = totutorialPosition;

        // 3) wait a frame (and physics step) to settle
        yield return new WaitForFixedUpdate();
        yield return null;

        // 4) re-enable physics/locomotion
        if (characterController != null) characterController.enabled = true;
        if (bodyTransformer != null) bodyTransformer.enabled = true;
        InputTracking.disablePositionalTracking = false;

        // 5) clear any residual velocity
        if (characterController != null)
            characterController.Move(Vector3.zero);

        // optional delay if you want a “teleport fade?effect
        if (torespawnDelay > 0f)
            yield return new WaitForSeconds(torespawnDelay);

        isRespawning = false;
    }

}