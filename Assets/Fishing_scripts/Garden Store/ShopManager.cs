using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ShopItemData
{
    public string id;         // unique identifier, e.g. "skybox_sunset"
    public string itemName;
    public Sprite itemIcon;
    public int price;
    // later you can add: public GameObject unlockPrefab; public Material skyboxMaterial;
}

public class ShopManager : MonoBehaviour
{
    [Header("Panel & Buttons")]
    public GameObject shopPanel;
    public Button openShopButton;

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

    // track selected item IDs and running total
    private HashSet<string> selectedIds = new HashSet<string>();
    private int runningTotal = 0;

    void Start()
    {
        shopPanel.SetActive(false);
        checkoutButton.interactable = false;
        openShopButton.onClick.AddListener(ToggleShop);
        checkoutButton.onClick.AddListener(OnCheckout);
        UpdateCurrencyDisplay();
    }

    public void ToggleShop()
    {
        bool open = !shopPanel.activeSelf;
        shopPanel.SetActive(open);
        checkoutButton.interactable = true;

        // hide any leftover message
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageText.gameObject.SetActive(false);
        

        if (open)
        {
            PopulateShop();
            UpdateCurrencyDisplay();
            UpdateTotalCostDisplay();
        }
        else 
        {
            checkoutButton.interactable = false;
        }
    }

    private void PopulateShop()
    {
        foreach (Transform t in shopListContent)
            Destroy(t.gameObject);

        foreach (var item in storeItems)
        {
            var rowObj = Instantiate(shopItemPrefab, shopListContent);
            var rowCtrl = rowObj.GetComponent<ShopItemRowController>();
            rowCtrl.Initialize(item, this);
        }
    }

    public void SelectItem(string id, int price)
    {
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
        int balance = PlayerPrefs.GetInt("PlayerCurrency", 0);
        if (balance >= runningTotal)
        {
            PlayerPrefs.SetInt("PlayerCurrency", balance - runningTotal);
            PlayerPrefs.Save();

            // TODO: unlock each selectedId (e.g. instantiate prefabs or set skybox)
            selectedIds.Clear();
            runningTotal = 0;

            ShowMessage("Purchase successful!", Color.green);

            UpdateCurrencyDisplay();
            UpdateTotalCostDisplay();
            PopulateShop(); // reset toggles
        }
        else
        {
            int needed = runningTotal - balance;
            ShowMessage($"Not enough Coins! Need {needed} more.", Color.red);
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
}
