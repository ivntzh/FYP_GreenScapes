using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PotSpawner : MonoBehaviourPun
{
    public GameObject potPrefab;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool hasSpawnedNew = false;

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        if (hasSpawnedNew || potPrefab == null) return;

        Debug.Log("🪴 Pot grabbed — spawning new one");
        PhotonNetwork.Instantiate(potPrefab.name, spawnPosition, spawnRotation);
        hasSpawnedNew = true;
    }
}
