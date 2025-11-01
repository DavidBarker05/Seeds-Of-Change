using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour, IEventListener
{
    [SerializeField]
    ItemScriptableObject emptyItem;
    [SerializeField, Min(0)]
    int startingMoney = 20;

    Dictionary<ItemScriptableObject, int> items = new Dictionary<ItemScriptableObject, int>();

    public Dictionary<ItemScriptableObject, int> Inventory => new Dictionary<ItemScriptableObject, int>(items);

    int _money;
    public int Money
    {
        get => _money;
        private set
        {
            _money = value;
            EventBus.Instance?.BroadcastEvent(GameEventType.MoneyChangedEvent, _money);
        }
    }

    void Awake()
    {
        Money = startingMoney;
        EventBus.Instance?.AddEventListener(GameEventType.ItemPurchasedEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.ItemSoldEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.GainCropEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.ItemStoredEvent, this);
    }

    private void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.ItemPurchasedEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.ItemSoldEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.GainCropEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.ItemStoredEvent, this);
    }

    public void AddItem(ItemScriptableObject item, int amount = 1)
    {
        if (item == null || amount <= 0) return;
        if (items.ContainsKey(item)) items[item] += amount;
        else items.Add(item, amount);
        EventBus.Instance?.BroadcastEvent(GameEventType.PlayerInventoryUpdateEvent, item, amount);
    }

    public bool TryRemoveItem(ItemScriptableObject item, int amount = 1)
    {
        if (!items.ContainsKey(item) || amount <= 0) return false;
        items[item] -= amount;
        if (items[item] <= 0) items.Remove(item);
        EventBus.Instance?.BroadcastEvent(GameEventType.PlayerInventoryUpdateEvent, item, -amount);
        return true;
    }

    public int TakeItemFromInventory(ItemScriptableObject item, int amount = 1)
    {
        if (!items.ContainsKey(item) || amount <= 0) return 0;
        int amountTaken = amount;
        if (items[item] - amount < 0) amountTaken = items[item];
        TryRemoveItem(item, amountTaken);
        return amountTaken;
    }

    public bool ContainsItem(ItemScriptableObject item) => items.ContainsKey(item);

    public int ItemAmount(ItemScriptableObject item) => items.ContainsKey(item) ? items[item] : 0;

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.ItemPurchasedEvent:
                if (parameters[0] is ItemScriptableObject purchasedItem)
                {
                    if (parameters[1] is int amount)
                    {
                        if (Money >= purchasedItem.PurchasePrice * amount)
                        {
                            AddItem(purchasedItem, amount);
                            Money -= purchasedItem.PurchasePrice * amount;
                            EventBus.Instance?.BroadcastEvent(GameEventType.ItemSpawnEvent, purchasedItem, amount);
                        }
                    }
                    else
                    {
                        if (Money <= purchasedItem.PurchasePrice) return;
                        AddItem(purchasedItem);
                        Money -= purchasedItem.PurchasePrice;
                        EventBus.Instance?.BroadcastEvent(GameEventType.ItemSpawnEvent, purchasedItem, 1);
                    }
                }
                break;
            case GameEventType.ItemSoldEvent:
                if (parameters[0] is ItemScriptableObject soldItem)
                {
                    if (parameters[1] is int amount && TryRemoveItem(soldItem, amount))
                    {
                        Money += soldItem.SellPrice * amount;
                        EventBus.Instance?.BroadcastEvent(GameEventType.SuccessfulSaleEvent, soldItem, amount);
                    }
                    else if (TryRemoveItem(soldItem))
                    {
                        Money += soldItem.SellPrice;
                        EventBus.Instance?.BroadcastEvent(GameEventType.SuccessfulSaleEvent, soldItem, 1);
                    }
                }
                break;
            case GameEventType.GainCropEvent:
                if (parameters[0] is CropScriptableObject crop && parameters[1] is bool isDead && parameters[2] is int currentGrowStage && parameters[3] is float currentYield) 
                {
                    if (currentGrowStage < 1) return;
                    ItemScriptableObject harvest = isDead ? crop.GrowthStages[currentGrowStage - 1].deadCrop.cropYield.yieldItem : crop.GrowthStages[currentGrowStage - 1].aliveCrop.cropYield.yieldItem;
                    if (harvest == emptyItem) return;
                    int amount = (int)((float)(isDead ? crop.GrowthStages[currentGrowStage - 1].deadCrop.cropYield.yieldAmount : crop.GrowthStages[currentGrowStage - 1].aliveCrop.cropYield.yieldAmount) * (currentYield / 100f));
                    if (amount <= 0) return;
                    AddItem(harvest, amount);
                }
                break;
            case GameEventType.ItemStoredEvent:
                if (parameters[0] is ItemScriptableObject storedItem)
                {
                    if (parameters[1] is int amount && TryRemoveItem(storedItem, amount)) EventBus.Instance?.BroadcastEvent(GameEventType.SuccessfulStoreEvent, storedItem, amount);
                    else if (TryRemoveItem(storedItem)) EventBus.Instance?.BroadcastEvent(GameEventType.SuccessfulStoreEvent, storedItem, 1);
                }
                break;
            default:
                break;
        }
    }
}
