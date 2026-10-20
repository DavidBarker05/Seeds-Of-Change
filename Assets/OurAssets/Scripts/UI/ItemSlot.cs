using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum SlotType
{
    Purchase,
    Sell,
    Store
}

public class ItemSlot : MonoBehaviour
{
    [SerializeField]
    SlotType m_SlotType;
    [SerializeField]
    TMP_Text m_ItemName;
    [SerializeField]
    TMP_Text m_Price;
    [SerializeField]
    TMP_InputField m_Quantity;
    [SerializeField]
    TMP_Text m_Total;
    [SerializeField]
    Button m_Button;

    ItemScriptableObject m_Item;
    int m_Amount;
    PlayerInventory m_PlayerInventory;

    void Awake()
    {
        m_Button.onClick.AddListener(
            () =>
            {

                if (int.TryParse(m_Quantity.text, out int amount) && amount > 0)
                {
                    string eventName = m_SlotType switch
                    {
                        SlotType.Purchase => "ItemPurchasedEvent",
                        SlotType.Sell => "ItemSoldEvent",
                        SlotType.Store => "ItemStoredEvent",
                        _ => throw new System.NotImplementedException($"{m_SlotType} is not implemented")
                    };
                    EventBus.Instance?.BroadcastEvent(eventName, m_Item, amount);
                }
            }
        );
        m_Quantity.onValueChanged.AddListener(QuantityChanged);
    }

    public void SetData(PlayerInventory playerInventory, ItemScriptableObject item)
    {
        m_PlayerInventory = playerInventory;
        m_Item = item;
        UpdateVisuals();
    }

    public void SetData(ItemScriptableObject item, int amount)
    {
        m_Item = item;
        m_Amount = amount;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        m_ItemName.text = m_SlotType switch
        {
            SlotType.Purchase => $"{m_Item.ItemName}",
            SlotType.Sell or SlotType.Store => $"{m_Item.ItemName} ({m_Amount})",
            _ => throw new System.NotImplementedException($"{m_SlotType} is not implemented")
        };
        if (m_SlotType != SlotType.Store)
        {
            m_Price.text = m_SlotType switch
            {
                SlotType.Purchase => $"{m_Item.PurchasePrice}",
                SlotType.Sell => $"{m_Item.SellPrice}",
                _ => throw new System.NotImplementedException($"{m_SlotType} is not implemented")
            };
            m_Total.text = "0";
        }
        m_Quantity.text = "0";
    }

    void QuantityChanged(string value)
    {
        m_Quantity.onValueChanged.RemoveAllListeners();
        int val = Mathf.Max(int.Parse(value), 0);
        val = m_SlotType switch
        {
            SlotType.Purchase => Mathf.Clamp(val, 0, val * m_Item.PurchasePrice / m_PlayerInventory.Money),
            SlotType.Sell or SlotType.Store => Mathf.Clamp(val, 0, m_Amount),
            _ => throw new System.NotImplementedException($"{m_SlotType} is not implemented")
        };
        if (m_Quantity.text != value) m_Quantity.text = value;
        if (m_SlotType == SlotType.Purchase) m_Total.text = $"{val * m_Item.PurchasePrice}";
        else if (m_SlotType == SlotType.Sell) m_Total.text = $"{val * m_Item.SellPrice}";
        m_Quantity.onValueChanged.AddListener(QuantityChanged);
    }
}
