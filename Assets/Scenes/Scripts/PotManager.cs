using Photon.Pun;
using UnityEngine;

public class PotManager : MonoBehaviourPun
{
    public GameObject flatDirt;
    public AudioSource dirtDropSound;

    private void Start()
    {
        flatDirt.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("DroppedDirt")) return;
        photonView.RPC(nameof(EnableDirt), RpcTarget.AllBuffered);
        Destroy(other.gameObject);
        enabled = false;
    }

    [PunRPC]
    private void EnableDirt()
    {
        flatDirt.SetActive(true);
        dirtDropSound.Play();
    }
}