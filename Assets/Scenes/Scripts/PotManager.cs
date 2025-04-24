using Photon.Pun;
using UnityEngine;

public class PotManager : MonoBehaviourPun
{
    public GameObject flatDirt;
    public AudioSource dirtDropSound;

    private void Start()
    {
        if (flatDirt != null)
            flatDirt.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DroppedDirt"))
        {
            Debug.Log("Dirt detected in pot!");

            photonView.RPC("EnableDirt", RpcTarget.AllBuffered);

            Destroy(other.gameObject);
            this.enabled = false;
        }
    }

    [PunRPC]
    private void EnableDirt()
    {
        if (flatDirt != null)
            flatDirt.SetActive(true);

        if (dirtDropSound != null)
            dirtDropSound.Play();
    }
}
