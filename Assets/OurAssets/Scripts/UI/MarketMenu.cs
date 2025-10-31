using UnityEngine;
using System.Collections.Generic;

public class MarketMenu : MonoBehaviour, IEventListener
{
    List<ItemScriptableObject> purchasableItems;
    Dictionary<ItemScriptableObject, int> sellableItems;

    void Start()
    {
        EventBus.Instance?.AddEventListener(GameEventType.PlayerInventoryUpdateEvent, this);
    }

    void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.IsPaused = true;
    }

    void OnDestroy() => EventBus.Instance?.RemoveEventListener(GameEventType.PlayerInventoryUpdateEvent, this);

    void UpdatePurchaseMenuList()
    {

    }

    void UpdateSellMenuList()
    {

    }

    public void PopulatePurchaseMenu(List<ItemScriptableObject> itemList)
    {
        purchasableItems = new List<ItemScriptableObject>(itemList);
        UpdatePurchaseMenuList();
    }

    public void PopulateSellMenu(Dictionary<ItemScriptableObject, int> itemDictionary)
    {
        sellableItems = new Dictionary<ItemScriptableObject, int>(itemDictionary);
        UpdateSellMenuList();
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.PlayerInventoryUpdateEvent:
                if (sellableItems == null) return;
                if (parameters[0] is ItemScriptableObject item && parameters[1] is int amount)
                {
                    if (!item.IsSellable) return;
                    if (sellableItems.ContainsKey(item))
                    {
                        sellableItems[item] += amount;
                        if (sellableItems[item] <= 0) sellableItems.Remove(item);
                    }
                    else if (amount > 0) sellableItems.Add(item, amount);
                }
                break;
            default:
                break;
        }
    }
}
