using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FamilyFoodMenu : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("storableItemSlotPrefab")]
    ItemSlot m_StorableItemSlotPrefab;
    [SerializeField, FormerlySerializedAs("storableContent")]
    RectTransform m_StorableContent;

    readonly Dictionary<ItemScriptableObject, int> m_SellableItems = new Dictionary<ItemScriptableObject, int>();
    readonly Dictionary<ItemScriptableObject, ItemSlot> m_StorableItemSlots = new Dictionary<ItemScriptableObject, ItemSlot>();

    void CreateSellableItemSlot(ItemScriptableObject item, int amount)
    {
        if (m_StorableItemSlots.ContainsKey(item)) return;
        ItemSlot slot = Instantiate(m_StorableItemSlotPrefab, m_StorableContent);
        slot.SetData(item, amount);
        slot.gameObject.SetActive(true);
        m_StorableItemSlots.Add(item, slot);
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
                Destroy(m_StorableItemSlots[item].gameObject);
                m_StorableItemSlots.Remove(item);
            }
            else m_StorableItemSlots[item].SetData(item, m_SellableItems[item]);
        }
        else if (amount > 0)
        {
            m_SellableItems.Add(item, amount);
            CreateSellableItemSlot(item, amount);
        }
    }
}
