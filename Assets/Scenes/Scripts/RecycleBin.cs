using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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

        // Ask the MasterClient to handle this specific trash instance
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

        var trashGO = tv.gameObject;
        var trash   = trashGO.GetComponent<TrashType>();
        if (trash == null)
        {
            Debug.LogError("[RecycleBin][Host] That view had no TrashType!");
            return;
        }

        bool isCorrect = (trash.trashCategory == acceptedTrashType);

        // 4) Transfer ownership so Host can destroy
        if (tv.OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
            tv.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        // 5) Destroy network-wide
        PhotonNetwork.Destroy(trashGO);

        // 6) Award & sync coins if correct
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

        // 7) Broadcast sound RPC to all clients
        photonView.RPC(
            nameof(RPC_PlayBinSound),
            RpcTarget.All,
            isCorrect
        );
    }

    [PunRPC]
    void RPC_PlayBinSound(bool correct)
    {
        // This runs on every client¡ªplays one-shot so BGM isn¡¯t touched
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
