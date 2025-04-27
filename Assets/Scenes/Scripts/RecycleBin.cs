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
        // 1) Only the MasterClient actually runs this
        if (!PhotonNetwork.IsMasterClient) return;

        // 2) Lookup the trash¡¯s PhotonView
        var tv = PhotonView.Find(trashViewID);
        if (tv == null)
        {
            Debug.LogError($"[Bin][Host] No PhotonView found for ID {trashViewID}");
            return;
        }

        // 3) Grab the TrashType and determine correctness
        var trashGO = tv.gameObject;
        var trash   = trashGO.GetComponent<TrashType>();
        if (trash == null)
        {
            Debug.LogError("[Bin][Host] That view had no TrashType!");
            return;
        }
        bool isCorrect = (trash.trashCategory == acceptedTrashType);

        // 5) TAKE OWNERSHIP on the host so Destroy is allowed
        if (tv.OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
            tv.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        // 6) Now we can safely destroy it network-wide
        PhotonNetwork.Destroy(trashGO);

        // 7) Award currency if correct
        if (isCorrect)
        {
            shopManager.AddCurrency(rewardAmount);
            shopManager.SyncDataToClients();
            shopManager.photonView.RPC(
                nameof(ShopManager.PlayCorrectSubmissionFeedbackRPC),
                RpcTarget.All,
                rewardAmount
            );
        }
    }
}
