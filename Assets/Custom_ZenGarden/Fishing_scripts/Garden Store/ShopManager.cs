using System.Collections;
using System.Collections.Generic;
using System.Linq; // for OrderBy
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class ShopItemData
{
    public string id;         // MUST match: 
                              // • Environment prefab GameObject name 
                              // • Skybox Material.name 
                              // • BGM AudioClip.name
    public string itemName;
    public Sprite itemIcon;
    public int price;
    // later you can add: public GameObject unlockPrefab; public Material skyboxMaterial;
}

public class ShopManager : MonoBehaviour
{
    private const string PURCHASED_KEY = "PurchasedItems";

    [Header("Testing (Inspector‑only)")]
    [Tooltip("Set to >= 0 to override PlayerCurrency on Start")]
    public int testCurrency = -1;

    [Header("Panel & Buttons")]
    public GameObject shopPanel;
    public Button openShopButton;
    public Button resetShopButton; 

    [Header("Message Settings")]
    public float messageDuration = 5f;   // seconds before auto‑hide
    private Coroutine messageCoroutine;  // tracks the running hide coroutine

    [Header("Scroll‑View Setup")]
    public Transform  shopListContent;
    public GameObject shopItemPrefab;
    public List<ShopItemData> storeItems;

    [Header("Currency & Totals")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI totalCostText;  
    public TextMeshProUGUI messageText;
    public Button checkoutButton;

    [Header("References")]
    public EnvironmentSettingsManager environmentSettingsManager;

    // track selected item IDs and running total
    private HashSet<string> selectedIds = new HashSet<string>();
    private int runningTotal = 0;

    private HashSet<string> purchasedIds = new HashSet<string>();

    void Start()
    {
        // ——— Testing override ———
        if (testCurrency >= 0)
        {
            PlayerPrefs.SetInt("PlayerCurrency", testCurrency);
            PlayerPrefs.Save();
        }
        
        LoadPurchased();
        shopPanel.SetActive(false);
        checkoutButton.interactable = false;
        openShopButton.onClick.AddListener(ToggleShop);
        checkoutButton.onClick.AddListener(OnCheckout);

        if (resetShopButton != null)
            resetShopButton.onClick.AddListener(ResetShop);

        UpdateCurrencyDisplay();
    }

    private void LoadPurchased()
    {
        string json = PlayerPrefs.GetString(PURCHASED_KEY, "");
        if (!string.IsNullOrEmpty(json))
            purchasedIds = new HashSet<string>(
                JsonUtility.FromJson<Serialization<string>>(json).ToList()
            );
    }

    private void SavePurchased()
    {
        var wrapper = new Serialization<string>(purchasedIds.ToList());
        PlayerPrefs.SetString(PURCHASED_KEY, JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    public void ToggleShop()
    {
        bool open = !shopPanel.activeSelf;
        shopPanel.SetActive(open);
        checkoutButton.interactable = open;

        // hide any leftover message
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageText.gameObject.SetActive(false);

        // ——— Clear out any previous selection ———
        selectedIds.Clear();
        runningTotal = 0;

        if (open)
        {
            PopulateShop();            // re‑create all rows (each will start unselected)
            UpdateCurrencyDisplay();   
            UpdateTotalCostDisplay();  // now shows “0 Coins” in green
        }
        else 
        {
            // shop closed, blank out the total
            totalCostText.text = "Total:";
        }
    }


    private void PopulateShop()
    {
        // Clear
        foreach (Transform t in shopListContent) Destroy(t.gameObject);

        // Sort: available first, then by price ascending
        var sorted = storeItems
            .OrderBy(item => purchasedIds.Contains(item.id));  // false (0) first, true (1) last
            //.ThenBy(item => item.price);

        foreach (var item in sorted)
        {
            var rowObj  = Instantiate(shopItemPrefab, shopListContent);
            var rowCtrl = rowObj.GetComponent<ShopItemRowController>();
            bool bought = purchasedIds.Contains(item.id);
            rowCtrl.Initialize(item, this, bought);
        }
    }

    public void SelectItem(string id, int price)
    {
        if (purchasedIds.Contains(id)) return;
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

    private void UpdateTotalCostDisplay()
    {
        int balance = PlayerPrefs.GetInt("PlayerCurrency", 0);
        int total   = runningTotal;

        // Choose a color name or hex code
        string colorTag = (balance >= total) ? "green" : "red";
        // Or use hex: e.g. "#00FF00" for green, "#FF0000" for red

        // Build the colored cost string
        string costStr      = $"{total} Coins";
        string coloredCost  = $"<color=\"{colorTag}\">{costStr}</color>";  // rich‑text tag :contentReference[oaicite:0]{index=0}

        // Finally, set the TMP text
        totalCostText.text = $"Total: \n{coloredCost}";
    }

    private void OnCheckout()
    {
        //if no items selected
        if (runningTotal == 0)
        {
            ShowMessage("Please select an item first.", Color.yellow);
            return;
        }

        int balance = PlayerPrefs.GetInt("PlayerCurrency", 0);
        if (balance >= runningTotal)
        {
            PlayerPrefs.SetInt("PlayerCurrency", balance - runningTotal);
            PlayerPrefs.Save();

            foreach (var id in selectedIds)
                purchasedIds.Add(id);
            SavePurchased();

            selectedIds.Clear();
            runningTotal = 0;

            ShowMessage("Purchase successful!", Color.green);
            UpdateCurrencyDisplay();
            UpdateTotalCostDisplay();
            PopulateShop();

            // **refresh environment settings**
            environmentSettingsManager.Refresh();
        }
        else
        {
            int need = runningTotal - balance;
            ShowMessage($"Not enough Coins! Need {need} more.", Color.red);
        }
    }

    private void UpdateCurrencyDisplay()
    {
        int balance = PlayerPrefs.GetInt("PlayerCurrency", 0);
        currencyText.text = $"Coins: {balance}";
    }

    /// <summary>
    /// Shows the shop message in the chosen color, then hides it after <see cref="messageDuration"/>.
    /// </summary>
    private void ShowMessage(string msg, Color color)
    {
        messageText.text = msg;
        messageText.color = color;
        messageText.gameObject.SetActive(true);

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }

        /// <summary>
    /// Clears all purchased items (and currency) for testing.
    /// </summary>
    public void ResetShop()
    {
        // Only MasterClient (host) can reset when connected
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            ShowMessage("Only the host can reset the shop.", Color.red);
            return;
        }

        // Clear local data
        currentPurchasedIds.Clear();
        currentCurrency = 0;
        selectedIds.Clear();
        runningTotal = 0;

        // Reset PlayerPrefs
        PlayerPrefs.DeleteKey(PURCHASED_KEY);
        PlayerPrefs.DeleteKey("PlayerCurrency");
        PlayerPrefs.Save();

        if (PhotonNetwork.IsConnected)
        {
            // Sync cleared data to all clients
            photonView.RPC("SyncShopDataRPC", RpcTarget.AllBuffered, 
                currentCurrency,
                JsonUtility.ToJson(new Serialization<string>(new List<string>()))
            );
        
            // Refresh environment for all players
            photonView.RPC("RefreshAllEnvironment", RpcTarget.All);
        }
        else
        {
            // Singleplayer refresh
            UpdateUI();
            environmentSettingsManager.RefreshEnvironment();
        }

        // Force UI rebuild
        PopulateShop();
        UpdateCurrencyDisplay();
        UpdateTotalCostDisplay();
    }

    [PunRPC]
    void RefreshAllEnvironment()
    {
        UpdateUI();
        environmentSettingsManager.RefreshEnvironment();
    }

    [System.Serializable]
    public class StringArray
    {
        public string[] items;
    }
}
