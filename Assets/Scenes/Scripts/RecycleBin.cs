using UnityEngine;
using Photon.Pun;

public class RecycleBin : MonoBehaviourPun
{
    public TrashCategory acceptedTrashType;
    public int           rewardAmount = 1;
    public ShopManager   shopManager;
    public AudioClip     correctSound;

    private void OnTriggerEnter(Collider other)
    {
        // MasterClient processes everything
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("Something entered the bin: " + other.name);

        var trash = other.GetComponentInParent<TrashType>();
        if (trash == null) return;

        bool isCorrect = (trash.trashCategory == acceptedTrashType);

        // Play sound on all clients
        photonView.RPC(nameof(PlayRecycleSoundRPC), RpcTarget.All, isCorrect);

        // Destroy networked trash if possible
        var trashGO = trash.gameObject;
        var pv      = trashGO.GetComponent<PhotonView>();
        if (pv != null)
            PhotonNetwork.Destroy(trashGO);
        else
            Destroy(trashGO);

        // Award currency if correct
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
    void PlayRecycleSoundRPC(bool correct)
    {
        if (correct && correctSound != null)
            AudioSource.PlayClipAtPoint(correctSound, transform.position);
        // else: you could play a ¡°wrong¡± clip here if desired
    }
}
