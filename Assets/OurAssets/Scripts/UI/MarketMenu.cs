using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MarketMenu : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("availableBalanceText")]
    TMP_Text m_AvailableBalanceText;
    [SerializeField, FormerlySerializedAs("purchasableItemSlotPrefab")]
    ItemSlot m_PurchasableItemSlotPrefab;
    [SerializeField, FormerlySerializedAs("purchasableContent")]
    RectTransform m_PurchasableContent;
    [SerializeField, FormerlySerializedAs("sellableItemSlotPrefab")]
    ItemSlot m_SellableItemSlotPrefab;
    [SerializeField, FormerlySerializedAs("sellableContent")]
    RectTransform m_SellableContent;
    [SerializeField, FormerlySerializedAs("purchaseMenu")]
    GameObject m_PurchaseMenu;
    [SerializeField, FormerlySerializedAs("sellMenu")]
    GameObject m_SellMenu;
    [SerializeField]
    PlayerInventory m_PlayerInventory;

    int m_AvailableBalance;
    List<ItemScriptableObject> m_PurchasableItems;
    readonly List<ItemSlot> m_PurchasableItemSlots = new List<ItemSlot>();
    readonly Dictionary<ItemScriptableObject, int> m_SellableItems = new Dictionary<ItemScriptableObject, int>();
    readonly Dictionary<ItemScriptableObject, ItemSlot> m_SellableItemSlots = new Dictionary<ItemScriptableObject, ItemSlot>();

    void OnEnable()
    {
        m_PurchaseMenu.SetActive(true);
        m_SellMenu.SetActive(false);
    }

    void CreateSellableItemSlot(ItemScriptableObject item, int amount)
    {
        if (m_SellableItemSlots != null && m_SellableItemSlots.ContainsKey(item)) return;
        ItemSlot slot = Instantiate(m_SellableItemSlotPrefab, m_SellableContent);
        slot.SetData(item, amount);
        slot.gameObject.SetActive(true);
        m_SellableItemSlots.Add(item, slot);
    }

    public void SetAvailableBalance(int money)
    {
        m_AvailableBalance = money;
        if (m_AvailableBalanceText != null) m_AvailableBalanceText.text = $"AVAILABLE BALANCE: {m_AvailableBalance}";
    }

    public void PopulatePurchaseMenu(List<ItemScriptableObject> itemList)
    {
        m_PurchasableItems = new List<ItemScriptableObject>(itemList);
        for (int i = m_PurchasableContent.childCount - 1; i >= 0; --i)
            Destroy(m_PurchasableContent.GetChild(i).gameObject);
        m_SellableItemSlots.Clear();
        foreach (ItemScriptableObject item in m_PurchasableItems)
        {
            ItemSlot slot = Instantiate(m_PurchasableItemSlotPrefab, m_PurchasableContent);
            slot.SetData(m_PlayerInventory, item);
            slot.gameObject.SetActive(true);
            m_PurchasableItemSlots.Add(slot);
        }
    }

    public void UpdatePlayerInventory(ItemScriptableObject item, int amount)
    {
        if (!item.IsSellable) return;
        if (m_SellableItems.ContainsKey(item))
        {
            m_SellableItems[item] += amount;
            if (m_SellableItems[item] <= 0)
            {
                m_SellableItems.Remove(item);
                Destroy(m_SellableItemSlots[item].gameObject);
                m_SellableItemSlots.Remove(item);
            }
            else m_SellableItemSlots[item].SetData(item, m_SellableItems[item]);
        }
        else if (amount > 0)
        {
            m_SellableItems.Add(item, amount);
            CreateSellableItemSlot(item, amount);
        }
    }
}
