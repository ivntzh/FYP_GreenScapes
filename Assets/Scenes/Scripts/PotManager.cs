// PotManager.cs
using Photon.Pun;
using UnityEngine;

public class PotManager : MonoBehaviourPun
{
    [Header("Flat Dirt Visual")]
    public GameObject flatDirt;      // Assign same ¡°flat dirt¡± you gave to Dirt.flatDirt
    public AudioSource dirtDropSound;

    void Start() => flatDirt?.SetActive(false);

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("DroppedDirt")) return;
        photonView.RPC(nameof(EnableDirt), RpcTarget.AllBuffered);
        Destroy(other.gameObject);
        enabled = false;  // stop until reset
    }

    [PunRPC]
    void EnableDirt()
    {
        flatDirt?.SetActive(true);
        dirtDropSound?.Play();
    }
}
