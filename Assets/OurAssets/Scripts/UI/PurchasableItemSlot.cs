using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PurchasableItemSlot : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI itemName;
    [SerializeField]
    TextMeshProUGUI purchaseCost;
    [SerializeField]
    TMP_InputField purchaseQuantity;
    [SerializeField]
    TextMeshProUGUI totalCost;
    [SerializeField]
    Button purchaseButton;

    ItemScriptableObject item;
    string lastPurchaseQuantityText;

    void Awake()
    {
        if (purchaseButton == null || purchaseQuantity == null) return;
        purchaseButton.onClick.AddListener(
            () => {
                if (int.TryParse(purchaseQuantity.text, out int amountSold) && amountSold > 0) EventBus.Instance?.BroadcastEvent(GameEventType.ItemPurchasedEvent, item, amountSold);
            }
        );
        purchaseQuantity.onValueChanged.AddListener(PurchaseQuantityChanged);
    }

    public void SetData(ItemScriptableObject item)
    {
        this.item = item;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        itemName.text = item.ItemName;
        purchaseCost.text = $"{item.PurchasePrice}";
        lastPurchaseQuantityText = "0";
        purchaseQuantity.text = "0";
        totalCost.text = "0";
    }

    void PurchaseQuantityChanged(string value)
    {
        purchaseQuantity.onValueChanged.RemoveAllListeners();
        string cleaned = System.Text.RegularExpressions.Regex.Replace(value, @"[^\d]", "");
        if (lastPurchaseQuantityText == "0" && cleaned.Length > 1) cleaned = cleaned.Replace("0", "");
        if (string.IsNullOrEmpty(cleaned)) cleaned = "0";
        if (purchaseQuantity.text != cleaned) purchaseQuantity.text = cleaned;
        if (int.TryParse(cleaned, out int amountSold))
        {
            purchaseQuantity.text = $"{amountSold}";
            totalCost.text = $"{amountSold * item.PurchasePrice}";
        }
        else totalCost.text = "0";
        purchaseQuantity.onValueChanged.AddListener(PurchaseQuantityChanged);
    }
}
