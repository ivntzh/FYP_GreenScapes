using UnityEngine;
using Photon.Pun;

public class NetworkFishingRod : MonoBehaviourPun
{
    [Header("Prefab Settings")]
    [Tooltip("Name of your rod prefab in Resources/NetworkFishingRod.prefab")]
    public string networkPrefabName = "NetworkFishingRod";

    // Holds your spawned network‐rod instance
    private GameObject _spawnedNetworkRod;

    void Awake()
    {
        // If this GameObject has a PhotonView, it's the NETWORKED copy.
        // Hide it for its owner so they only ever see their LOCAL rod.
        var pv = GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>())  c.enabled = false;
        }
    }

    /// <summary>
    /// Call this from your XRGrabInteractable → On Select Entered (in Inspector)
    /// </summary>
    public void SpawnNetworkRod()
    {
        // Only run on the LOCAL rod (which has no PhotonView)
        if (GetComponent<PhotonView>() != null) return;

        if (_spawnedNetworkRod == null)
        {
            _spawnedNetworkRod = PhotonNetwork.Instantiate(
                networkPrefabName,
                transform.position,
                transform.rotation
            );
        }
    }

    /// <summary>
    /// Call this from your XRGrabInteractable → On Select Exited (in Inspector)
    /// </summary>
    public void DespawnNetworkRod()
    {
        // Only run on the LOCAL rod
        if (GetComponent<PhotonView>() != null) return;

        if (_spawnedNetworkRod != null)
        {
            PhotonNetwork.Destroy(_spawnedNetworkRod);
            _spawnedNetworkRod = null;
        }
    }
}
