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

public class ShopManager : MonoBehaviourPunCallbacks
{
    private const string PURCHASED_KEY = "PurchasedItems";
    
    // Added Photon data tracking
    private int currentCurrency;
    private HashSet<string> currentPurchasedIds = new HashSet<string>();
    private bool usingHostData = false;

    [Header("Testing (Inspector‑only)")]
    public int testCurrency = -1;

    [Header("Panel & Buttons")]
    public GameObject shopPanel;
    public Button openShopButton;
    public Button resetShopButton;

    [Header("Message Settings")]
    public float messageDuration = 5f;
    private Coroutine messageCoroutine;

    [Header("Scroll‑View Setup")]
    public Transform shopListContent;
    public GameObject shopItemPrefab;
    public List<ShopItemData> storeItems;

    [Header("Currency & Totals")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI totalCostText;
    public TextMeshProUGUI messageText;
    public Button checkoutButton;

    [Header("References")]
    public EnvironmentSettingsManager environmentSettingsManager;

    private HashSet<string> selectedIds = new HashSet<string>();
    private int runningTotal = 0;

    void Start()
    {
        // Initialize Photon View if missing
        if (!GetComponent<PhotonView>())
        {
            var pv = gameObject.AddComponent<PhotonView>();
            pv.ObservedComponents = new List<Component> { this };
        }

        // Testing override
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
            }
        }
        else
        {
            LoadLocalData();
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
        usingHostData = true;
        UpdateUI();
    }

    void SyncDataToClients()
    {
        var wrapper = new Serialization<string>(currentPurchasedIds.ToList());
        string json = JsonUtility.ToJson(wrapper);
        photonView.RPC("SyncShopDataRPC", RpcTarget.OthersBuffered, currentCurrency, json);
    }

    public void ToggleShop()
    {
        bool open = !shopPanel.activeSelf;
        shopPanel.SetActive(open);
        checkoutButton.interactable = open;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageText.gameObject.SetActive(false);

        selectedIds.Clear();
        runningTotal = 0;

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
        if (selectedIds.Add(id))
        {
            runningTotal += price;
            UpdateTotalCostDisplay();
        }
    }

    public void DeselectItem(string id, int price)
    {
        if (selectedIds.Remove(id))
        {
            runningTotal -= price;
            UpdateTotalCostDisplay();
        }
    }

    void UpdateTotalCostDisplay()
    {
        int balance = currentCurrency;
        int total = runningTotal;
        string colorTag = (balance >= total) ? "green" : "red";
        string costStr = $"{total} Coins";
        string coloredCost = $"<color=\"{colorTag}\">{costStr}</color>";
        totalCostText.text = $"Total: \n{coloredCost}";
    }

    public void OnCheckout()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RequestPurchaseRPC", RpcTarget.MasterClient, selectedIds.ToArray());
            return;
        }

        ProcessPurchase();
    }

    [PunRPC]
    void RequestPurchaseRPC(string[] itemIds, PhotonMessageInfo info)
    {
        int total = itemIds.Sum(id => storeItems.Find(i => i.id == id).price);
        if (currentCurrency >= total)
        {
            currentCurrency -= total;
            foreach (string id in itemIds) currentPurchasedIds.Add(id);
            SaveLocalData();
            photonView.RPC("PurchaseSuccessRPC", info.Sender);
            SyncDataToClients();
        }
        else
        {
            photonView.RPC("PurchaseFailedRPC", info.Sender, "Not enough coins!");
        }
    }

    void ProcessPurchase()
    {
        if (runningTotal == 0)
        {
            ShowMessage("Please select an item first.", Color.yellow);
            return;
        }

        if (currentCurrency >= runningTotal)
        {
            currentCurrency -= runningTotal;
            foreach (var id in selectedIds)
                currentPurchasedIds.Add(id);
            
            SaveLocalData();
            selectedIds.Clear();
            runningTotal = 0;

            ShowMessage("Purchase successful!", Color.green);
            UpdateUI();
            environmentSettingsManager.Refresh();

            if (PhotonNetwork.IsConnected)
                SyncDataToClients();
        }
        else
        {
            int need = runningTotal - currentCurrency;
            ShowMessage($"Not enough Coins! Need {need} more.", Color.red);
        }
    }

    [PunRPC]
    void PurchaseSuccessRPC()
    {
        selectedIds.Clear();
        runningTotal = 0;
        ShowMessage("Purchase successful!", Color.green);
        UpdateUI();
    }

    [PunRPC]
    void PurchaseFailedRPC(string message)
    {
        ShowMessage(message, Color.red);
    }

    void UpdateUI()
    {
        UpdateCurrencyDisplay();
        PopulateShop();
        UpdateTotalCostDisplay();
    }

    void UpdateCurrencyDisplay()
    {
        currencyText.text = $"Coins: {currentCurrency}";
    }

    void ShowMessage(string msg, Color color)
    {
        messageText.text = msg;
        messageText.color = color;
        messageText.gameObject.SetActive(true);

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }

    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }

    public void ResetShop()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        PlayerPrefs.DeleteKey(PURCHASED_KEY);
        PlayerPrefs.DeleteKey("PlayerCurrency");
        PlayerPrefs.Save();

        currentPurchasedIds.Clear();
        currentCurrency = 0;

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncShopDataRPC", RpcTarget.AllBuffered, currentCurrency, 
                JsonUtility.ToJson(new Serialization<string>(new List<string>())));
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}