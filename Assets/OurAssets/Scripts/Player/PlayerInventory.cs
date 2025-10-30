using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    Dictionary<ItemScriptableObject, int> items = new Dictionary<ItemScriptableObject, int>();

    public Dictionary<ItemScriptableObject, int> GetInventory => new Dictionary<ItemScriptableObject, int>(items);

    public void AddItem(ItemScriptableObject item, int amount = 1)
    {
        if (item == null) return;
        int amountAdded = Mathf.Max(amount, 1);
        if (items.ContainsKey(item)) items[item] += amountAdded;
        else items.Add(item, amountAdded);
        EventBus.Instance?.BroadcastEvent(GameEventType.PlayerInventoryUpdate, item, amountAdded);
    }

    public bool TryRemoveItem(ItemScriptableObject item, int amount = 1)
    {
        if (!items.ContainsKey(item)) return false;
        int amountRemoved = Mathf.Max(amount, 1);
        items[item] -= amountRemoved;
        if (items[item] <= 0) items.Remove(item);
        EventBus.Instance?.BroadcastEvent(GameEventType.PlayerInventoryUpdate, item, -amountRemoved);
        return true;
    }

    public int TakeItemFromInventory(ItemScriptableObject item, int amount = 1)
    {
        if (!items.ContainsKey(item)) return 0;
        int amountTaken = Mathf.Max(amount, 1);
        if (items[item] - amount < 0) amountTaken = items[item];
        TryRemoveItem(item, amountTaken);
        return amountTaken;
    }

    public bool ContainsItem(ItemScriptableObject item) => items.ContainsKey(item);

    public int ItemAmount(ItemScriptableObject item) => items.ContainsKey(item) ? items[item] : 0;
}
