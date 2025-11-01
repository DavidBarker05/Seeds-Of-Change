using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FamilyFoodMenu : MonoBehaviour
{
    [SerializeField]
    StorableItemSlot storableItemSlotPrefab;
    [SerializeField]
    RectTransform storableContent;

    Dictionary<ItemScriptableObject, int> sellableItems = new Dictionary<ItemScriptableObject, int>();
    Dictionary<ItemScriptableObject, StorableItemSlot> storableItemSlots = new Dictionary<ItemScriptableObject, StorableItemSlot>();

    void CreateSellableItemSlot(ItemScriptableObject item, int amount)
    {
        if (storableItemSlots != null && storableItemSlots.ContainsKey(item)) return;
        StorableItemSlot slot = Instantiate(storableItemSlotPrefab, storableContent);
        slot.SetData(item, amount);
        slot.gameObject.SetActive(true);
        storableItemSlots.Add(item, slot);
    }

    public void UpdatePlayerInventory(ItemScriptableObject item, int amount)
    {
        if (!item.IsSellable) return;
        if (sellableItems.ContainsKey(item))
        {
            sellableItems[item] += amount;
            if (sellableItems[item] <= 0)
            {
                sellableItems.Remove(item);
                Destroy(storableItemSlots[item].gameObject);
                storableItemSlots.Remove(item);
            }
            else storableItemSlots[item].SetData(item, sellableItems[item]);
        }
        else if (amount > 0)
        {
            sellableItems.Add(item, amount);
            CreateSellableItemSlot(item, amount);
        }
    }
}
