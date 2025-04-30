using UnityEngine;
using Photon.Pun;

public class NetworkFishingRod : MonoBehaviourPun
{
    [Header("Prefab Settings")]
<<<<<<< HEAD
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
=======
    public string networkPrefabName = "NetworkFishingRod";

    private GameObject _spawnedNetworkRod;

    [Header("Optional")]
    public Transform followTarget; // Set this to the local rod in inspector or script

    void Awake()
    {
        if (photonView != null && photonView.IsMine)
        {
            // Hide mesh and colliders for the owner's networked copy
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;
        }
    }

    void Update()
    {
        // Only the networked copy that I own should follow the local rod
        if (photonView != null && photonView.IsMine && followTarget != null)
        {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }

    public void SpawnNetworkRod()
    {
        // Only run on the local rod (which has no PhotonView)
>>>>>>> MultiplayerPlanting
        if (GetComponent<PhotonView>() != null) return;

        if (_spawnedNetworkRod == null)
        {
            _spawnedNetworkRod = PhotonNetwork.Instantiate(
                networkPrefabName,
                transform.position,
                transform.rotation
            );
<<<<<<< HEAD
        }
    }

    /// <summary>
    /// Call this from your XRGrabInteractable → On Select Exited (in Inspector)
    /// </summary>
    public void DespawnNetworkRod()
    {
        // Only run on the LOCAL rod
=======

            // Assign follow target to the spawned object's script
            var netRodScript = _spawnedNetworkRod.GetComponent<NetworkFishingRod>();
            if (netRodScript != null)
            {
                netRodScript.followTarget = this.transform; // This is the local rod
            }
        }
    }

    public void DespawnNetworkRod()
    {
        // Only run on the local rod
>>>>>>> MultiplayerPlanting
        if (GetComponent<PhotonView>() != null) return;

        if (_spawnedNetworkRod != null)
        {
            PhotonNetwork.Destroy(_spawnedNetworkRod);
            _spawnedNetworkRod = null;
<<<<<<< HEAD
        }
    }
}
=======
        }
    }
}
>>>>>>> MultiplayerPlanting
