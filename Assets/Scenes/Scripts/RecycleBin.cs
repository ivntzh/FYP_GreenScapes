using UnityEngine;
using Photon.Pun;

public class RecycleBin : MonoBehaviourPun
{
    public TrashCategory acceptedTrashType;
    public int rewardAmount = 1;
    public ShopManager shopManager;
    public AudioClip correctSound;

    private void OnTriggerEnter(Collider other)
    {
        // 1) Find trash type
        var trash = other.GetComponentInParent<TrashType>();
        if (trash == null) return;

        // 2) Ask the host to process this trash
        var tv = trash.GetComponent<PhotonView>();
        if (tv != null)
            photonView.RPC(
                nameof(RPC_ProcessTrash),
                RpcTarget.MasterClient,
                tv.ViewID
            );
        else
            Debug.LogError("Trash had no PhotonView!");
    }

    [PunRPC]
    void RPC_ProcessTrash(int trashViewID, PhotonMessageInfo info)
    {
        // 3) Only the host actually runs this
        if (!PhotonNetwork.IsMasterClient) return;

        var tv = PhotonView.Find(trashViewID);
        if (tv == null) return;
        var trashGO = tv.gameObject;
        var trash   = trashGO.GetComponent<TrashType>();
        bool isCorrect = trash.trashCategory == acceptedTrashType;

        // 4) Play the sound on everyone
        photonView.RPC(
            nameof(RPC_PlayBinSound),
            RpcTarget.All,
            isCorrect
        );

        // 5) Destroy the trash
        PhotonNetwork.Destroy(trashGO);

        // 6) Award points if correct
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
        if (correct && correctSound != null)
            AudioSource.PlayClipAtPoint(correctSound, transform.position);
        // else: play a ¡°wrong¡± clip or UI toast if you like
    }
}
