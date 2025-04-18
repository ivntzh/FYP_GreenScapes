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

    private int currentCurrency;
    public HashSet<string> currentPurchasedIds = new HashSet<string>();
    private HashSet<string> selectedIds = new HashSet<string>();
    private int runningTotal = 0;

    // Required empty implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Empty but required for PhotonView observation
    }

    void Start()
    {
        // Ensure PhotonView exists on this GameObject
        if (GetComponent<PhotonView>() == null)
        {
            gameObject.AddComponent<PhotonView>();
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
                Debug.Log($"Master loaded {currentPurchasedIds.Count} purchased items");
                SyncDataToClients();
            
                // Force immediate environment update
                environmentSettingsManager.RefreshEnvironment();
            }
        }
        else
        {
            LoadLocalData();
            Debug.Log($"Offline loaded {currentPurchasedIds.Count} purchased items");
        
            // Directly trigger environment refresh
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
    
    // Add this line to force environment refresh
    if (environmentSettingsManager != null)
    {
        environmentSettingsManager.RefreshEnvironment();
        if (PhotonNetwork.IsConnected)
        {
            environmentSettingsManager.photonView.RPC("RefreshEnvironmentRPC", RpcTarget.Others);
        }
    }
}

    void SyncDataToClients()
    {
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
        if (runningTotal == 0)
        {
            ShowMessage("Please select an item first.", Color.yellow);
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ProcessPurchase();
            }
            else
            {
                // Convert array to comma-separated string
                string itemsString = string.Join(",", selectedIds.ToArray());
                photonView.RPC("RequestPurchaseRPC", RpcTarget.MasterClient, itemsString);
            }
        }
        else
        {
            ProcessPurchase();
        }
    }

    [PunRPC]
    void RequestPurchaseRPC(string itemsString)
    {
        // Convert back to array
        string[] itemIds = itemsString.Split(',');

        int total = itemIds.Sum(id => storeItems.Find(i => i.id == id).price);
        if (currentCurrency >= total)
        {
            currentCurrency -= total;
            foreach (string id in itemIds) currentPurchasedIds.Add(id);
            SaveLocalData();
        
            // Send success response
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
            foreach (var id in selectedIds)
                currentPurchasedIds.Add(id);
        
            SaveLocalData();
            SyncDataToClients();
        
            // Convert to string for RPC
            string itemsString = string.Join(",", selectedIds.ToArray());
            photonView.RPC("PurchaseSuccessRPC", RpcTarget.All, itemsString);
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
        // Clear selection for all clients
        selectedIds.Clear();
        runningTotal = 0;
    
        // Update local state
        string[] itemIds = purchasedItems.Split(',');
        foreach (string id in itemIds)
        {
            currentPurchasedIds.Add(id);
        }
    
        ShowMessage("Purchase successful!", Color.green);
        UpdateUI();
        environmentSettingsManager.RefreshEnvironment();
    }

    [PunRPC]
    void PurchaseFailedRPC(string message)
    {
        ShowMessage(message, Color.red);
        UpdateUI();
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