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

    private void OnTriggerEnter(Collider other)
    {
        // 1) All clients detect any trash overlap
        var trashComponent = other.GetComponentInParent<TrashType>();
        if (trashComponent == null) return;

        var trashPV = trashComponent.GetComponent<PhotonView>();
        if (trashPV == null)
        {
            Debug.LogError("[RecycleBin] Trash has no PhotonView!", trashComponent);
            return;
        }

        // 2) RPC to the MasterClient to process this specific trash ViewID
        photonView.RPC(
            nameof(RPC_ProcessTrash),
            RpcTarget.MasterClient,
            trashPV.ViewID
        );
    }

    [PunRPC]
    void RPC_ProcessTrash(int trashViewID, PhotonMessageInfo info)
    {
        // Only the MasterClient runs the authoritative logic
        if (!PhotonNetwork.IsMasterClient) return;

        // Find the networked trash by its ViewID
        var tv = PhotonView.Find(trashViewID);
        if (tv == null)
        {
            Debug.LogError($"[RecycleBin][Host] No PhotonView found for ID {trashViewID}");
            return;
        }

        var trashGO = tv.gameObject;
        var trash   = trashGO.GetComponent<TrashType>();
        if (trash == null)
        {
            Debug.LogError("[RecycleBin][Host] That view had no TrashType!");
            return;
        }

        // Did they dump the correct category?
        bool isCorrect = (trash.trashCategory == acceptedTrashType);

        // 3) Transfer ownership so Destroy will succeed
        if (tv.OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
            tv.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        // 4) Destroy the trash network-wide
        PhotonNetwork.Destroy(trashGO);

        if (isCorrect)
        {
            // 5a) Award coins on the host
            shopManager.AddCurrency(rewardAmount);
            shopManager.SyncDataToClients();

            // 5b) Show the UI toast on everyone (host+clients)
            shopManager.photonView.RPC(
                nameof(ShopManager.CurrencyAddedConfirmationRPC),
                RpcTarget.All,
                rewardAmount
            );

            // 6a) Play correct sound locally on each client
            if (correctSound != null)
                AudioSource.PlayClipAtPoint(correctSound, transform.position);
        }
        else
        {
            // Wrong bin ¡ú just play failure sound
            if (wrongSound != null)
                AudioSource.PlayClipAtPoint(wrongSound, transform.position);
        }
    }
}
