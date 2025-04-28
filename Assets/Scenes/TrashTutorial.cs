using UnityEngine;

public class TrashTutorial : MonoBehaviour
{
    public TrashCategory acceptedTrashType;
    public AudioClip correctSound;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the bin: " + other.name);

        TutorialTrashType trash = other.GetComponentInParent<TutorialTrashType>();
        if (trash != null)
        {
            if (trash.trashCategory == acceptedTrashType)
            {
                // Correct bin
                if (correctSound != null)
                    AudioSource.PlayClipAtPoint(correctSound, transform.position);

                if (TutorialTrashManager.Instance != null)
                {
                    TutorialTrashManager.Instance.TrashDestroyed(); // Tell manager one trash was correctly handled
                }

                Destroy(trash.gameObject); // ✅ Destroy parent object and DO NOT respawn now
            }
            else
            {
                // Wrong bin
                Vector3 originalPosition = trash.spawnPosition;

                Destroy(trash.gameObject);

                // Respawn immediately
                GameObject newTrash = Instantiate(trash.prefabReference, originalPosition, Quaternion.identity);
            }
        }
    }
}
