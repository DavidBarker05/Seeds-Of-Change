using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellableItemSlot : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI itemName;
    [SerializeField]
    TextMeshProUGUI sellAmount;
    [SerializeField]
    TMP_InputField sellQuantity;
    [SerializeField]
    TextMeshProUGUI totalSellAmount;
    [SerializeField]
    Button sellButton;

    ItemScriptableObject item;
    int amount;

    void Awake()
    {
        if (sellButton == null || sellQuantity == null) return;
        sellButton.onClick.AddListener(
            () => {
                if (int.TryParse(sellQuantity.text, out int amountSold) && amountSold > 0) EventBus.Instance?.BroadcastEvent(GameEventType.ItemSoldEvent, item, amountSold);
            }
        );
        sellQuantity.onValueChanged.AddListener(SellQuantityChanged);
    }

    public void SetData(ItemScriptableObject item, int amount)
    {
        this.item = item;
        this.amount = amount;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        itemName.text = $"{item.ItemName} ({amount})";
        sellAmount.text = $"{item.SellPrice}";
        sellQuantity.text = "0";
        totalSellAmount.text = "0";
    }

    void SellQuantityChanged(string value)
    {
        sellQuantity.onValueChanged.RemoveAllListeners();
        string cleaned = System.Text.RegularExpressions.Regex.Replace(value, @"[^\d]", "");
        if (string.IsNullOrEmpty(cleaned)) cleaned = "0";
        if (sellQuantity.text != cleaned) sellQuantity.text = cleaned;
        if (int.TryParse(cleaned, out int amountSold))
        {
            amountSold = Mathf.Min(amountSold, amount);
            sellQuantity.text = $"{amountSold}";
            totalSellAmount.text = $"{amountSold * item.SellPrice}";
        }
        else totalSellAmount.text = "0";
        sellQuantity.onValueChanged.AddListener(SellQuantityChanged);
    }
}
