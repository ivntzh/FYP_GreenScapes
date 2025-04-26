using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

[System.Serializable]
public class ShopItemData
{
    public string id;
    public string itemName;
    public Sprite itemIcon;
    public int price;
}

public class ShopManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private const string PURCHASED_KEY = "PurchasedItems";

    [Header("Testing (Inspector‑only)")]
    public int testCurrency = -1;

    [Header("Panel & Buttons")]
    public GameObject shopPanel;
    public Button openShopButton;
    public Button resetShopButton;
    public GameObject resetPanel;

    [Header("Message Settings")]
    public float messageDuration = 5f;
    private Coroutine messageCoroutine;

    [Header("Scroll‑View Setup")]
    public Transform shopListContent;
    public GameObject shopItemPrefab;
    public List<ShopItemData> storeItems;

    [Header("Currency & Totals")]
    // Make public so other scripts can read
    public int currentCurrency;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI totalCostText;
    public TextMeshProUGUI messageText;
    public Button checkoutButton;

    [Header("References")]
    public EnvironmentSettingsManager environmentSettingsManager;

    [Header("Audio Clips")]
    public AudioClip correctSubmissionAudioClip;
    public AudioClip wrongSubmissionAudioClip;

    // Purchased and selected IDs
    public HashSet<string> currentPurchasedIds = new HashSet<string>();
    private HashSet<string> selectedIds = new HashSet<string>();
    private int runningTotal = 0;

    // Required empty implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Empty but required
    }

    void Start()
    {
        if (GetComponent<PhotonView>() == null)
            gameObject.AddComponent<PhotonView>();

        if (testCurrency >= 0)
        {
            PlayerPrefs.SetInt("PlayerCurrency", testCurrency);
            PlayerPrefs.Save();
        }

        InitializeData();
        shopPanel.SetActive(false);
        checkoutButton.interactable = false;
        openShopButton.onClick.AddListener(ToggleShop);
        checkoutButton.onClick.AddListener(OnCheckout);
        if (resetShopButton != null)
            resetShopButton.onClick.AddListener(ResetShop);

        UpdateCurrencyDisplay();
    }

    void InitializeData()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadLocalData();
                SyncDataToClients();
                environmentSettingsManager.RefreshEnvironment();
            }
        }
        else
        {
            LoadLocalData();
            environmentSettingsManager.RefreshEnvironment();
        }
    }

    void LoadLocalData()
    {
        currentCurrency = PlayerPrefs.GetInt("PlayerCurrency", 0);
        string json = PlayerPrefs.GetString(PURCHASED_KEY, "");
        if (!string.IsNullOrEmpty(json))
            currentPurchasedIds = new HashSet<string>(
                JsonUtility.FromJson<Serialization<string>>(json).ToList()
            );
    }

    void SaveLocalData()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            PlayerPrefs.SetInt("PlayerCurrency", currentCurrency);
            var wrapper = new Serialization<string>(currentPurchasedIds.ToList());
            PlayerPrefs.SetString(PURCHASED_KEY, JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
        }
    }

    [PunRPC]
    void SyncShopDataRPC(int currency, string purchasedJson)
    {
        currentCurrency = currency;
        currentPurchasedIds = new HashSet<string>(
            JsonUtility.FromJson<Serialization<string>>(purchasedJson).ToList()
        );
        UpdateUI();
        environmentSettingsManager.RefreshEnvironment();
        if (PhotonNetwork.IsConnected)
            environmentSettingsManager.photonView.RPC("RefreshEnvironmentRPC", RpcTarget.Others);
    }

    public void SyncDataToClients()
    {
        var fishGM = FindObjectOfType<FishingGameManager>();
        if (fishGM != null) fishGM.UpdateCurrencyUI(currentCurrency);

        if (!PhotonNetwork.IsMasterClient) return;
        var wrapper = new Serialization<string>(currentPurchasedIds.ToList());
        string json = JsonUtility.ToJson(wrapper);
        photonView.RPC("SyncShopDataRPC", RpcTarget.OthersBuffered, currentCurrency, json);
    }

    public void ToggleShop()
    {
        bool open = !shopPanel.activeSelf;
        shopPanel.SetActive(open);
        checkoutButton.interactable = open;
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageText.gameObject.SetActive(false);
        selectedIds.Clear(); runningTotal = 0;
        if (open)
        {
            PopulateShop();
            UpdateCurrencyDisplay();
            UpdateTotalCostDisplay();
        }
        else
        {
            totalCostText.text = "Total:";
        }
    }

    void PopulateShop()
    {
        foreach (Transform t in shopListContent) Destroy(t.gameObject);
        var sorted = storeItems
            .OrderBy(item => currentPurchasedIds.Contains(item.id))
            .ThenBy(item => item.price);
        foreach (var item in sorted)
        {
            var rowObj = Instantiate(shopItemPrefab, shopListContent);
            var rowCtrl = rowObj.GetComponent<ShopItemRowController>();
            bool bought = currentPurchasedIds.Contains(item.id);
            rowCtrl.Initialize(item, this, bought);
        }
    }

    public void SelectItem(string id, int price)
    {
        if (currentPurchasedIds.Contains(id)) return;
        if (selectedIds.Add(id)) { runningTotal += price; UpdateTotalCostDisplay(); }
    }

    public void DeselectItem(string id, int price)
    {
        if (selectedIds.Remove(id)) { runningTotal -= price; UpdateTotalCostDisplay(); }
    }

    void UpdateTotalCostDisplay()
    {
        int balance = currentCurrency;
        int total = runningTotal;
        string color = (balance >= total) ? "green" : "red";
        totalCostText.text = $"Total:\n<color={color}>{total} Coins</color>";
    }

    public void OnCheckout()
    {
        if (runningTotal == 0)
        {
            ShowMessage("Please select an item first.", Color.yellow);
            return;
        }
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
                ProcessPurchase();
            else
                photonView.RPC("RequestPurchaseRPC", RpcTarget.MasterClient, string.Join(",", selectedIds));
        }
        else
        {
            ProcessPurchase();
        }
    }

    [PunRPC]
    void RequestPurchaseRPC(string itemsString)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        string[] ids = itemsString.Split(',');
        int total = ids.Sum(id => storeItems.Find(i => i.id == id).price);
        if (currentCurrency >= total)
        {
            currentCurrency -= total;
            foreach (var id in ids) currentPurchasedIds.Add(id);
            SaveLocalData();
            photonView.RPC("PurchaseSuccessRPC", RpcTarget.All, itemsString);
            SyncDataToClients();
        }
        else
        {
            int need = total - currentCurrency;
            photonView.RPC("PurchaseFailedRPC", RpcTarget.All, $"Not enough coins! Need {need} more.");
        }
    }

    void ProcessPurchase()
    {
        int total = runningTotal;
        if (currentCurrency >= total)
        {
            currentCurrency -= total;
            foreach (var id in selectedIds) currentPurchasedIds.Add(id);
            SaveLocalData();
            SyncDataToClients();
            photonView.RPC("PurchaseSuccessRPC", RpcTarget.All, string.Join(",", selectedIds));
        }
        else
        {
            int need = total - currentCurrency;
            photonView.RPC("PurchaseFailedRPC", RpcTarget.All, $"Not enough Coins! Need {need} more.");
        }
    }

    [PunRPC]
    void PurchaseSuccessRPC(string purchasedItems)
    {
        selectedIds.Clear(); runningTotal = 0;
        foreach (var id in purchasedItems.Split(',')) currentPurchasedIds.Add(id);
        ShowMessage("Purchase successful!", Color.green);
        UpdateUI();
        environmentSettingsManager.RefreshEnvironment();
    }

    [PunRPC]
    void PurchaseFailedRPC(string message)
    {
        ShowMessage(message, Color.red);
        selectedIds.Clear(); runningTotal = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        UpdateCurrencyDisplay();
        PopulateShop();
        UpdateTotalCostDisplay();
    }

    public void UpdateCurrencyDisplay()
    {
        currencyText.text = $"Coins: {currentCurrency}";
    }

    void ShowMessage(string msg, Color color)
    {
        messageText.text = msg;
        messageText.color = color;
        messageText.gameObject.SetActive(true);
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }

    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }

    public void ResetShop()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            ShowMessage("Only the host can reset the shop.", Color.red);
            return;
        }
        currentPurchasedIds.Clear(); currentCurrency = 0; selectedIds.Clear(); runningTotal = 0;
        PlayerPrefs.DeleteKey(PURCHASED_KEY);
        PlayerPrefs.DeleteKey("PlayerCurrency");
        PlayerPrefs.Save();
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncShopDataRPC", RpcTarget.AllBuffered,
                currentCurrency,
                JsonUtility.ToJson(new Serialization<string>(new List<string>())))
            ;
            photonView.RPC("RefreshAllEnvironment", RpcTarget.All);
        }
        else
        {
            UpdateUI(); environmentSettingsManager.RefreshEnvironment();
        }
        PopulateShop(); UpdateCurrencyDisplay(); UpdateTotalCostDisplay();
        resetPanel.SetActive(false);
    }

    [PunRPC]
    void RefreshAllEnvironment()
    {
        UpdateUI();
        environmentSettingsManager.RefreshEnvironment();
    }

    // === NEW Multiplayer Currency RPCs ===
    [PunRPC]
    private void RequestAddCurrencyRPC(int amount, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentCurrency += amount;
        SaveLocalData();
        SyncDataToClients();
        photonView.RPC("CurrencyAddedConfirmationRPC", RpcTarget.Others, amount);
    }

    [PunRPC]
    public void CurrencyAddedConfirmationRPC(int amount)
    {
        ShowMessage($"Gained {amount} Coins!", Color.green);
        UpdateUI();
    }

    // Public helper for host to add currency
    public void AddCurrency(int amount)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentCurrency += amount;
            SaveLocalData();
            SyncDataToClients();
        }
    }

    [PunRPC]
    public void PlayCorrectSubmissionFeedbackRPC(int amount)
    {
        // Play correct sound
        if (correctSubmissionAudioClip != null)
            AudioSource.PlayClipAtPoint(correctSubmissionAudioClip, transform.position);

        // Show success message
        ShowMessage($"+  {amount}  coins!", Color.green);
        UpdateUI();
    }

    [PunRPC]
    public void PlayWrongSubmissionFeedbackRPC()
    {
        // Play wrong sound
        if (wrongSubmissionAudioClip != null)
            AudioSource.PlayClipAtPoint(wrongSubmissionAudioClip, transform.position);

        // Show failure message
        ShowMessage("Wrong plant!", Color.red);
    }

    [System.Serializable]
    public class StringArray { public string[] items; }
}
