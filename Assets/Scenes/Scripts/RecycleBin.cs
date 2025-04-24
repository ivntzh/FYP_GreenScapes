using UnityEngine;
using Photon.Pun;

public class RecycleBin : MonoBehaviour
{
    public TrashCategory acceptedTrashType;
    public int rewardAmount = 1;
    public ShopManager shopManager;

    public AudioClip correctSound;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the bin: " + other.name);

        TrashType trash = other.GetComponentInParent<TrashType>();
        if (trash != null)
        {
            if (trash.trashCategory == acceptedTrashType)
            {
                if (correctSound != null)
                    AudioSource.PlayClipAtPoint(correctSound, transform.position);

                AwardCurrency();
                Destroy(trash.gameObject);
            }
            else
            {
                // Wrong bin handling will go here later
                Destroy(trash.gameObject);
            }
        }
    }

    private void AwardCurrency()
    {
        if (shopManager != null)
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    shopManager.AddCurrency(rewardAmount);
                }
                else
                {
                    shopManager.photonView.RPC("RequestAddCurrencyRPC", RpcTarget.MasterClient, rewardAmount);
                }
            }
            else
            {
                shopManager.AddCurrency(rewardAmount);
                shopManager.UpdateCurrencyDisplay();
            }
        }
    }
}
