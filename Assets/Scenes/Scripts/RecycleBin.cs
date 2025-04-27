using UnityEngine;
using Photon.Pun;

public class RecycleBin : MonoBehaviourPun
{
    [Header("Bin Settings")]
    public TrashCategory acceptedTrashType;
    public int rewardAmount = 1;

    [Header("Feedback")]
    public ShopManager shopManager;
    public AudioClip correctSound;
    public AudioClip wrongSound; // optional ¡°failure¡± sound

    private void OnTriggerEnter(Collider other)
    {
        // 1) All clients detect the overlap
        var trashComponent = other.GetComponentInParent<TrashType>();
        if (trashComponent == null) return;

        var trashPV = trashComponent.GetComponent<PhotonView>();
        if (trashPV == null)
        {
            Debug.LogError("[Bin] Trash has no PhotonView!", trashComponent);
            return;
        }

        // 2) Tell the host which view to process
        photonView.RPC(
            nameof(RPC_ProcessTrash),
            RpcTarget.MasterClient,
            trashPV.ViewID
        );
    }

    [PunRPC]
    void RPC_ProcessTrash(int trashViewID, PhotonMessageInfo info)
    {
        // 3) Only the MasterClient actually runs this logic
        if (!PhotonNetwork.IsMasterClient) return;

        var tv = PhotonView.Find(trashViewID);
        if (tv == null)
        {
            Debug.LogError($"[Bin][Host] No PhotonView found for ID {trashViewID}");
            return;
        }

        var trashGO = tv.gameObject;
        var trash   = trashGO.GetComponent<TrashType>();
        if (trash == null)
        {
            Debug.LogError("[Bin][Host] That view had no TrashType!");
            return;
        }

        // 4) Determine correctness
        bool isCorrect = (trash.trashCategory == acceptedTrashType);

        // 5) Broadcast the appropriate sound & toast to everyone
        photonView.RPC(
            nameof(RPC_PlayBinSound),
            RpcTarget.All,
            isCorrect
        );

        // 6) Destroy the trash network©\wide
        PhotonNetwork.Destroy(trashGO);

        // 7) Award currency if it was correct
        if (isCorrect)
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.SyncDataToClients();
            shopManager.photonView.RPC(
                nameof(ShopManager.CurrencyAddedConfirmationRPC),
                RpcTarget.All,
                rewardAmount
            );
        }
    }

    [PunRPC]
    void RPC_PlayBinSound(bool correct)
    {
        // 8) Play the matching clip locally
        if (correct)
        {
            if (correctSound != null)
                AudioSource.PlayClipAtPoint(correctSound, transform.position);
        }
        else
        {
            if (wrongSound != null)
                AudioSource.PlayClipAtPoint(wrongSound, transform.position);
        }
    }
}
