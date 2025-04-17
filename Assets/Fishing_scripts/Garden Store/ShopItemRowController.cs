using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemRowController : MonoBehaviour
{
    [Header("UI References")]
    public Image              itemIcon;     // your product icon
    public TextMeshProUGUI    nameText;
    public TextMeshProUGUI    priceText;
    public Button             addButton;    // the button you click
    public Image              buttonIcon;   // the little icon on the right
    [Header("Toggle Sprites")]
    public Sprite             plusSprite;   // e.g. “+”
    public Sprite             crossSprite;  // e.g. “×”

    // runtime
    private ShopItemData      data;
    private ShopManager       manager;
    private bool              isBought;
    private bool              isSelected;

    /// <summary>
    /// Called by ShopManager.PopulateShop(...)
    /// </summary>
    public void Initialize(ShopItemData item, ShopManager mgr, bool bought)
    {
        data      = item;
        manager   = mgr;
        isBought  = bought;
        isSelected = false;

        // fill in the UI
        itemIcon.sprite    = item.itemIcon;
        nameText.text      = item.itemName;
        priceText.text     = $"{item.price} Coins";

        addButton.onClick.RemoveAllListeners();

        if (isBought)
        {
            // already owned → disable entirely
            addButton.interactable = false;
            buttonIcon.gameObject.SetActive(false);
            nameText.color         = Color.gray;
            priceText.color        = Color.gray;
        }
        else
        {
            // not yet owned → set to plus icon and hook toggle
            addButton.interactable = true;
            buttonIcon.gameObject.SetActive(true);
            buttonIcon.sprite      = plusSprite;
            addButton.onClick.AddListener(ToggleSelection);
        }
    }

    /// <summary>
    /// Called when the addButton is clicked
    /// </summary>
    private void ToggleSelection()
    {
        if (!isSelected)
        {
            // select
            manager.SelectItem(data.id, data.price);
            buttonIcon.sprite = crossSprite;
            isSelected = true;
        }
        else
        {
            // deselect
            manager.DeselectItem(data.id, data.price);
            buttonIcon.sprite = plusSprite;
            isSelected = false;
        }
    }
}
