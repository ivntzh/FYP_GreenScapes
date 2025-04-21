using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RubbishBin_fish : MonoBehaviour
{
    public FishingArea fishingArea;

    private void OnTriggerEnter(Collider other)
    {
        // Make sure your rubbish prefab is tagged "fishing_rubbish"
        if (other.CompareTag("fishing_rubbish"))
        {
            Destroy(other.gameObject);
            fishingArea.OnRubbishDisposed();
        }
    }
}
