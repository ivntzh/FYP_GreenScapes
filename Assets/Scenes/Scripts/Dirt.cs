using UnityEngine;
using Photon.Pun;

public class Dirt : MonoBehaviourPun
{
    public GameObject flatDirt;
    public GameObject mixedDirt;
    public MonoBehaviour potManager;
    public AudioSource mixSound;
    private bool isMixed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isMixed || !other.CompareTag("Rake")) return;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
                ProcessMix();
            else
                photonView.RPC(nameof(RequestMixRPC), RpcTarget.MasterClient);
        }
        else
        {
            ProcessMix();
        }
    }

    [PunRPC]
    private void RequestMixRPC(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ProcessMix();
        photonView.RPC(nameof(SyncMixRPC), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    private void SyncMixRPC()
    {
        ProcessMix();
    }

    private void ProcessMix()
    {
        isMixed = true;
        flatDirt?.SetActive(false);
        mixedDirt?.SetActive(true);
        if (potManager != null) potManager.enabled = false;
        mixSound?.Play();
        Debug.Log("Soil mixed (tilled).");
    }
}
