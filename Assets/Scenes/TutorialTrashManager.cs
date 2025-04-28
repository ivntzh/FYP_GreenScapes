using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrashManager : MonoBehaviour
{
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
        totalTrashDestroyed = 0; // Reset counter
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

}