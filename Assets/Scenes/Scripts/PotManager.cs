using UnityEngine;
using Photon.Pun;

public class PotManager : MonoBehaviourPun
{
    public GameObject flatDirt;
    public AudioSource dirtDropSound;
    private bool hasSoil = false;

    void Start()
    {
        flatDirt?.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasSoil && other.CompareTag("DroppedDirt"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsMasterClient)
                    ProcessSoilDrop();
                else
                    photonView.RPC(nameof(RequestSoilDropRPC), RpcTarget.MasterClient);
            }
            else
            {
                ProcessSoilDrop();
            }
        }
    }

    [PunRPC]
    public void RequestSoilDropRPC(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ProcessSoilDrop();
        photonView.RPC(nameof(SyncSoilDropRPC), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    public void SyncSoilDropRPC()
    {
        ProcessSoilDrop();
    }

    private void ProcessSoilDrop()
    {
        hasSoil = true;
        flatDirt?.SetActive(true);
        dirtDropSound?.Play();
        Debug.Log("Dirt added to pot.");
        Destroy(GameObject.FindWithTag("DroppedDirt"));
        this.enabled = false; // disable further triggers
    }
}
