using UnityEngine;

public class TrashTutorial : MonoBehaviour
{
    public TrashCategory acceptedTrashType;
    public AudioClip correctSound;
    public AudioClip errorSound; // 🎵 Assign an error sound in Inspector

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
                    TutorialTrashManager.Instance.TrashDestroyed(); // Count correct trash
                }

                Destroy(trash.gameObject); // Destroy correctly sorted trash
            }
            else
            {
                // ❌ Wrong bin

                if (errorSound != null)
                    AudioSource.PlayClipAtPoint(errorSound, transform.position); // Play error sound here 🎵❗

                if (TutorialTrashManager.Instance != null)
                {
                    TutorialTrashManager.Instance.RespawnTrashImmediately(trash);
                }

                Destroy(trash.gameObject); // Destroy wrong trash
            }
        }
    }
}
