using System.Collections.Generic;
using UnityEngine;

public class FamilyFoodStorage : Interactable, IEventListener
{
    [SerializeField]
    List<ItemScriptableObject> storableItems = new List<ItemScriptableObject>();
    [SerializeField]
    FamilyFoodMenu foodMenu;

    private void Awake()
    {
        List<ItemScriptableObject> _sellableItems = new List<ItemScriptableObject>(storableItems);
        foreach (ItemScriptableObject item in _sellableItems)
        {
            if (!item.IsSellable) storableItems.Remove(item);
        }
        EventBus.Instance?.AddEventListener(GameEventType.PlayerInventoryUpdateEvent, this);
    }

    void OnDestroy() => EventBus.Instance?.RemoveEventListener(GameEventType.PlayerInventoryUpdateEvent, this);

    public override bool Interact(params object[] parameters)
    {
        if (foodMenu != null && GameManager.Instance != null)
        {
            GameManager.Instance.IsPaused = true;
            foodMenu.gameObject.SetActive(true);
        }
        return true;
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.PlayerInventoryUpdateEvent:
                if (storableItems == null) return;
                if (parameters[0] is ItemScriptableObject item && parameters[1] is int amount) foodMenu?.UpdatePlayerInventory(item, amount);
                break;
            default:
                break;
        }
    }
}
