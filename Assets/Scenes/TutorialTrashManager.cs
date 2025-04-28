using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrashManager : MonoBehaviour
{
    [System.Serializable]
    public class TrashData
    {
        public GameObject prefab;           // Prefab to instantiate later
        public Vector3 spawnPosition;        // Original starting position
    }

    public static TutorialTrashManager Instance;

    public List<TrashData> allTrashList = new List<TrashData>();

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
        RegisterAllTrashInScene();
    }

    private void RegisterAllTrashInScene()
    {
        TutorialTrashType[] allTrash = FindObjectsOfType<TutorialTrashType>();

        foreach (TutorialTrashType trash in allTrash)
        {
            TrashData data = new TrashData();
            data.prefab = trash.prefabReference; // Link prefab manually inside trash
            data.spawnPosition = trash.transform.position;

            allTrashList.Add(data);
        }

        totalTrash = allTrashList.Count;
        Debug.Log($"Registered {totalTrash} trash objects at start.");
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

        foreach (TrashData data in allTrashList)
        {
            Instantiate(data.prefab, data.spawnPosition, Quaternion.identity);
        }

        totalTrashDestroyed = 0; // Reset counter for next round
    }
}