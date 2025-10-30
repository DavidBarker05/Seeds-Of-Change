using UnityEngine;
using System.Collections.Generic;

public abstract class ItemContainer : Interactable
{
    [SerializeField]
    protected ItemContainerScriptableObject containerData;

    protected readonly Dictionary<ItemScriptableObject, int> acceptedItems = new Dictionary<ItemScriptableObject, int>();
    protected int currentCapacity = 0;

    public override bool Interact(params object[] parameters)
    {
        if (containerData == null) Debug.LogError($"ERROR: ItemContainer needs a valid ItemContainerScriptableObject");
        if (parameters.Length != 1)
        {
            #if UNITY_EDITOR
                Debug.LogWarning($"WARNING: ItemContainer objects need 1 parameter. Received {parameters.Length} parameter(s)");
            #endif
        }
        else
        {
            if (parameters[0] is ItemScriptableObject item)
            {
                if (containerData?.AcceptableItems.Contains(item) ?? false)
                {
                    if (parameters[1] is int amount) return AddItem(item, amount);
                    else return AddItem(item);
                }
            }
            else if (parameters[0] is bool clearItems) ClearItems(); // This will be used for stuff like harvesting
            else
            {
                #if UNITY_EDITOR
                    if (parameters[0] is not ItemScriptableObject && parameters[0] is not bool) Debug.LogWarning($"WARNING: Parameter 0 needs to be an ItemScriptableObject or a bool. Received {parameters[0]} type {parameters[0].GetType()} as parameter 0");
                #endif
            }
        }
        return false;
    }

    public bool AddItem(ItemScriptableObject item, int amount = 1)
    {
        if (containerData == null || currentCapacity >= containerData.ContainerCapacity) return false;
        ExtraAddLogic(item);
        int adjustedAmount = amount;
        if (currentCapacity + amount > containerData.ContainerCapacity) adjustedAmount = currentCapacity + amount - containerData.ContainerCapacity;
        if (acceptedItems.ContainsKey(item))
        {
            acceptedItems[item] += adjustedAmount;
            currentCapacity += adjustedAmount;
        }
        else
        {
            acceptedItems.Add(item, adjustedAmount);
            currentCapacity += adjustedAmount;
        }
        return true;
    }

    public void ClearItems()
    {
        ExtraClearLogic();
        acceptedItems.Clear();
        currentCapacity = 0;
    }

    protected abstract void ExtraAddLogic(ItemScriptableObject item);

    protected abstract void ExtraClearLogic();
}
