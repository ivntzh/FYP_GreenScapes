using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemRowController : MonoBehaviour
{
    [Header("UI References")]
    public Image       iconImage;      // child "Icon"
    public TextMeshProUGUI nameText;   // child "ItemName"
    public TextMeshProUGUI priceText;  // child "ItemPrice"
    public Button      toggleButton;   // child "AddItem"
    public Image       toggleIcon;     // the Image component on that button

    [Header("Toggle Sprites")]
    public Sprite      plusSprite;     // assign your “+” icon
    public Sprite      crossSprite;    // assign your “×” icon

    // Internal
    private ShopItemData data;
    private ShopManager manager;
    private bool        isSelected = false;

    /// <summary>
    /// Call this from ShopManager when creating the row.
    /// </summary>
    public void Initialize(ShopItemData itemData, ShopManager shopManager)
    {
        data    = itemData;
        manager = shopManager;

        iconImage.sprite     = data.itemIcon;
        nameText.text        = data.itemName;
        priceText.text       = $"{data.price} Coins";
        toggleButton.onClick.AddListener(OnToggleClicked);

        SetSelected(false);
    }

    private void OnToggleClicked()
    {
        SetSelected(!isSelected);

        if (isSelected)
            manager.SelectItem(data.id, data.price);
        else
            manager.DeselectItem(data.id, data.price);
    }

    private void SetSelected(bool selected)
    {
        isSelected = selected;
        // Swap the button’s icon sprite at runtime :contentReference[oaicite:0]{index=0}
        toggleIcon.sprite = isSelected ? crossSprite : plusSprite;
    }
}
