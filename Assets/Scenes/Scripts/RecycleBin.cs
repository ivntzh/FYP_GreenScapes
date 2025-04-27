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
        var trash = other.GetComponentInParent<TrashType>();
        if (trash == null) return;

        var tv = trash.GetComponent<PhotonView>();
        if (tv == null)
        {
            Debug.LogError("[Bin] Trash has no PhotonView!", trash);
            return;
        }

        Debug.Log($"[Bin] Detected trash {tv.ViewID} on client {PhotonNetwork.LocalPlayer.ActorNumber}, RPC ¡ú host");
        photonView.RPC(nameof(RPC_ProcessTrash), RpcTarget.MasterClient, tv.ViewID);
    }

    [PunRPC]
    void RPC_ProcessTrash(int trashViewID, PhotonMessageInfo info)
    {
        Debug.Log($"[Bin][Host] RPC_ProcessTrash got ViewID={trashViewID} (sender={info.Sender})");
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[Bin][Host] But I¡¯m not MasterClient!");
            return;
        }

        var tv = PhotonView.Find(trashViewID);
        if (tv == null)
        {
            Debug.LogError($"[Bin][Host] Couldn¡¯t find PhotonView with ID={trashViewID}");
            return;
        }

        var trashGO = tv.gameObject;
        Debug.Log($"[Bin][Host] Destroying trash GO {trashGO.name}");
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
