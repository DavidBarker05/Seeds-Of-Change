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
        EventBus.Instance?.AddEventListener(GameEventType.PlayerInventoryUpdateEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.MoneyChangedEvent, this);
    }

    void Start() => marketMenu?.PopulatePurchaseMenu(purchasableItems);

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.PlayerInventoryUpdateEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.MoneyChangedEvent, this);
    }

    public override InteractionInfo Interact(params object[] parameters)
    {
        if (marketMenu != null && GameManager.Instance != null)
        {
            GameManager.Instance.IsPaused = true;
            marketMenu.gameObject.SetActive(true);
        }
        return new InteractionInfo() { DoEndInteraction = true };
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.PlayerInventoryUpdateEvent:
                if (sellableItems == null) return;
                if (parameters[0] is ItemScriptableObject item && parameters[1] is int amount) marketMenu?.UpdatePlayerInventory(item, amount);
                break;
            case GameEventType.MoneyChangedEvent:
                if (parameters[0] is int newMoney) marketMenu?.SetAvailableBalance(newMoney);
                break;
            default:
                break;
        }
    }
}
