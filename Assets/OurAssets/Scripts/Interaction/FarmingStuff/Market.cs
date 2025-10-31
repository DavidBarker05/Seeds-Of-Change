using System.Collections.Generic;
using UnityEngine;

public class Market : Interactable, IEventListener
{
    [SerializeField]
    List<ItemScriptableObject> purchasableItems = new List<ItemScriptableObject>();
    [SerializeField]
    List<ItemScriptableObject> sellableItems = new List<ItemScriptableObject>();
    [SerializeField]
    MarketMenu marketMenu;

    bool hasRequestedPlayerInventory = false;

    private void Awake()
    {
        List<ItemScriptableObject> _purchasableItems = new List<ItemScriptableObject>(purchasableItems);
        foreach (ItemScriptableObject item in _purchasableItems)
        {
            if (!item.IsPurchasable) purchasableItems.Remove(item);
        }
        List<ItemScriptableObject> _sellableItems = new List<ItemScriptableObject>(sellableItems);
        foreach (ItemScriptableObject item in _sellableItems)
        {
            if (!item.IsSellable) sellableItems.Remove(item);
        }
    }

    void Start()
    {
        EventBus.Instance?.AddEventListener(GameEventType.PlayerInventoryReceiveEvent, this);
        marketMenu?.PopulatePurchaseMenu(purchasableItems);
    }

    void OnDestroy() => EventBus.Instance?.RemoveEventListener(GameEventType.PlayerInventoryReceiveEvent, this);

    public override bool Interact(params object[] parameters)
    {
        if (marketMenu != null)
        {
            if (!hasRequestedPlayerInventory)
            {
                hasRequestedPlayerInventory = true;
                EventBus.Instance?.BroadcastEvent(GameEventType.PlayerInventoryRequestEvent, gameObject);
            }
            else marketMenu.gameObject.SetActive(true);
        }
        return true;
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.PlayerInventoryReceiveEvent:
                if (parameters[0] is GameObject requester && requester == gameObject && parameters[1] is Dictionary<ItemScriptableObject, int> inventory)
                {
                    Dictionary<ItemScriptableObject, int> sellableInventory = new Dictionary<ItemScriptableObject, int>();
                    foreach (KeyValuePair<ItemScriptableObject, int> kvp in inventory)
                    {
                        if (kvp.Key.IsSellable) sellableInventory.Add(kvp.Key, kvp.Value);
                    }
                    marketMenu?.PopulateSellMenu(sellableInventory);
                    marketMenu?.gameObject.SetActive(true);
                }
                break;
            default:
                break;
        }
    }
}
