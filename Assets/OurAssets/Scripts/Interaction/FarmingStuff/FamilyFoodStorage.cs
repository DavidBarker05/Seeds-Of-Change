using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FamilyFoodStorage : Interactable, IEventListener
{
    [SerializeField, FormerlySerializedAs("storableItems")]
    List<ItemScriptableObject> m_StorableItems;
    [SerializeField, FormerlySerializedAs("foodMenu")]
    FamilyFoodMenu m_FoodMenu;

    private void Awake()
    {
        List<ItemScriptableObject> _sellableItems = new List<ItemScriptableObject>(m_StorableItems);
        foreach (ItemScriptableObject item in _sellableItems)
        {
            if (!item.IsSellable) m_StorableItems.Remove(item);
        }
        EventBus.Instance?.AddEventListener("PlayerInventoryUpdateEvent", this);
    }

    void OnDestroy() => EventBus.Instance?.RemoveEventListener("PlayerInventoryUpdateEvent", this);

    public override InteractionInfo Interact(params object[] parameters)
    {
        return new InteractionInfo()
        {
            EndInteraction = true,
            OutArguments = new object[]
            {
                m_FoodMenu
            }
        };
    }

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        if (eventName == "PlayerInventoryUpdateEvent")
        {
            if (parameters[0] is ItemScriptableObject item && parameters[1] is int amount) m_FoodMenu.UpdatePlayerInventory(item, amount);
        }
    }
}
