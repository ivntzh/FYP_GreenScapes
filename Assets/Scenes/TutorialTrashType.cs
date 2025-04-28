using UnityEngine;

public class TutorialTrashType : MonoBehaviour
{
    public TrashCategory trashCategory;
    public GameObject prefabReference; // Assign manually (drag your original prefab)

    [HideInInspector]
    public Vector3 spawnPosition; // Store starting position for wrong bin respawn

    private void Start()
    {
        if (prefabReference == null)
        {
            prefabReference = this.gameObject; // Fallback, but not recommended
            Debug.LogWarning($"PrefabReference not set for {gameObject.name}. Using self instead.");
        }

        spawnPosition = transform.position; // Save original spawn position when game starts
    }
}