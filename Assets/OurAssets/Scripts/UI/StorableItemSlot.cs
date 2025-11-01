using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StorableItemSlot : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI itemName;
    [SerializeField]
    TMP_InputField storeQuantity;
    [SerializeField]
    Button storeButton;

    ItemScriptableObject item;
    int amount;
    string lastStoreQuantityText;

    void Awake()
    {
        if (storeButton == null || storeQuantity == null) return;
        storeButton.onClick.AddListener(
            () => {
                if (int.TryParse(storeQuantity.text, out int amountSold) && amountSold > 0) EventBus.Instance?.BroadcastEvent(GameEventType.ItemStoredEvent, item, amountSold);
            }
        );
        storeQuantity.onValueChanged.AddListener(StoreQuantityChanged);
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
        lastStoreQuantityText = "0";
        storeQuantity.text = "0";
    }

    void StoreQuantityChanged(string value)
    {
        storeQuantity.onValueChanged.RemoveAllListeners();
        string cleaned = System.Text.RegularExpressions.Regex.Replace(value, @"[^\d]", "");
        if (lastStoreQuantityText == "0" && cleaned.Length > 1) cleaned = cleaned.Substring(cleaned.Length - 1);
        if (string.IsNullOrEmpty(cleaned)) cleaned = "0";
        if (storeQuantity.text != cleaned) storeQuantity.text = cleaned;
        if (int.TryParse(cleaned, out int amountSold))
        {
            amountSold = Mathf.Min(amountSold, amount);
            storeQuantity.text = $"{amountSold}";
        }
        storeQuantity.onValueChanged.AddListener(StoreQuantityChanged);
    }
}
