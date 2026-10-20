using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Market : Interactable, IEventListener
{
    [SerializeField, FormerlySerializedAs("purchasableItems")]
    List<ItemScriptableObject> m_PurchasableItems;
    [SerializeField, FormerlySerializedAs("sellableItems")]
    List<ItemScriptableObject> m_SellableItems;
    [SerializeField, FormerlySerializedAs("marketMenu")]
    MarketMenu m_MarketMenu;

    private void Awake()
    {
        EventBus.Instance?.AddEventListener("PlayerInventoryUpdateEvent", this);
        EventBus.Instance?.AddEventListener("MoneyChangedEvent", this);
        List<ItemScriptableObject> _purchasableItems = new List<ItemScriptableObject>(m_PurchasableItems);
        foreach (ItemScriptableObject item in _purchasableItems)
        {
            if (!item.IsPurchasable) m_PurchasableItems.Remove(item);
        }
        List<ItemScriptableObject> _sellableItems = new List<ItemScriptableObject>(m_SellableItems);
        foreach (ItemScriptableObject item in _sellableItems)
        {
            if (!item.IsSellable) m_SellableItems.Remove(item);
        }
    }

    void Start() => m_MarketMenu.PopulatePurchaseMenu(m_PurchasableItems);

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener("PlayerInventoryUpdateEvent", this);
        EventBus.Instance?.RemoveEventListener("MoneyChangedEvent", this);
    }

    public override InteractionInfo Interact(params object[] parameters)
    {
        return new InteractionInfo()
        {
            EndInteraction = true,
            OutArguments = new object[]
            {
                m_MarketMenu
            }
        };
    }

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        switch (eventName)
        {
            case "PlayerInventoryUpdateEvent":
                if (m_SellableItems == null) return;
                if (parameters[0] is ItemScriptableObject item && parameters[1] is int amount) m_MarketMenu.UpdatePlayerInventory(item, amount);
                break;
            case "MoneyChangedEvent":
                if (parameters[0] is int newMoney) m_MarketMenu.SetAvailableBalance(newMoney);
                break;
            default:
                break;
        }
    }
}
