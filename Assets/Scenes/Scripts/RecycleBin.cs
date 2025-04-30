using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RecycleBin : MonoBehaviourPun
{
    [Header("Bin Settings")]
    public TrashCategory acceptedTrashType;
    public int           rewardAmount = 1;

    [Header("References")]
    public ShopManager   shopManager;

    [Header("Sounds")]
    public AudioClip     correctSound;
    public AudioClip     wrongSound;

    // Prevent double-processing the same trash
    private HashSet<int> processedTrashIDs = new HashSet<int>();

    private void OnTriggerEnter(Collider other)
    {
        var trashComponent = other.GetComponentInParent<TrashType>();
        if (trashComponent == null) return;

        var trashPV = trashComponent.GetComponent<PhotonView>();
        if (trashPV == null)
        {
            Debug.LogError("[RecycleBin] Trash has no PhotonView!", trashComponent);
            return;
        }

        // Ask MasterClient to handle scoring & destruction
        photonView.RPC(
            nameof(RPC_ProcessTrash),
            RpcTarget.MasterClient,
            trashPV.ViewID
        );
    }

    [PunRPC]
    void RPC_ProcessTrash(int trashViewID, PhotonMessageInfo info)
    {
        // 1) Only MasterClient runs the authoritative logic
        if (!PhotonNetwork.IsMasterClient) return;

        // 2) Prevent double-awarding
        if (processedTrashIDs.Contains(trashViewID))
            return;
        processedTrashIDs.Add(trashViewID);

        // 3) Find the trash¡¯s PhotonView & component
        var tv = PhotonView.Find(trashViewID);
        if (tv == null)
        {
            Debug.LogError($"[RecycleBin][Host] No PhotonView for ID {trashViewID}");
            return;
        }

        var trash = tv.GetComponent<TrashType>();
        if (trash == null)
        {
            Debug.LogError("[RecycleBin][Host] That view had no TrashType!");
            return;
        }

        bool isCorrect = (trash.trashCategory == acceptedTrashType);

        // 4) Award & sync coins if correct
        if (isCorrect)
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.SyncDataToClients();

            // Show UI toast on everyone
            shopManager.photonView.RPC(
                nameof(ShopManager.CurrencyAddedConfirmationRPC),
                RpcTarget.All,
                rewardAmount
            );
        }

        // 5) Broadcast sound RPC to all clients
        photonView.RPC(
            nameof(RPC_PlayBinSound),
            RpcTarget.All,
            isCorrect
        );

        // 6) Tell the **owner** of that trash object to destroy it
        var owner = tv.Owner;
        if (owner != null)
        {
            photonView.RPC(
                nameof(RPC_DestroyTrash),
                owner,
                trashViewID
            );
        }
        else
        {
            // Fallback if owner left: MasterClient can destroy
            PhotonNetwork.Destroy(tv.gameObject);
        }
    }

    [PunRPC]
    void RPC_DestroyTrash(int trashViewID, PhotonMessageInfo info)
    {
        var tv = PhotonView.Find(trashViewID);
        if (tv != null && tv.IsMine)
        {
            PhotonNetwork.Destroy(tv.gameObject);
        }
    }

    [PunRPC]
    void RPC_PlayBinSound(bool correct)
    {
        // Plays on every client¡ªwon¡¯t interfere with BGM
        var clip = correct ? correctSound : wrongSound;
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}
