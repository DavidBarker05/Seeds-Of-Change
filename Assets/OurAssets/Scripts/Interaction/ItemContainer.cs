using System.Collections.Generic;
using UnityEngine;

public struct AddItemInfo
{
    public bool WasAdded { get; set; }
}

public abstract class ItemContainer : Interactable
{
    [SerializeField]
    protected ItemContainerScriptableObject containerData;

    protected readonly Dictionary<ItemScriptableObject, int> acceptedItems = new Dictionary<ItemScriptableObject, int>();
    protected int currentCapacity = 0;

    public override InteractionInfo Interact(params object[] parameters)
    {
        if (containerData == null) Debug.LogError($"ERROR: ItemContainer needs a valid ItemContainerScriptableObject");
        InteractionInfo interactionInfo = new InteractionInfo()
        {
            EndInteraction = true
        };
        if (parameters.Length != 1 && parameters.Length != 2)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: ItemContainer objects need 1 or 2 parameters. Received {parameters.Length} parameter(s)");
#endif
        }
        else
        {
            if (parameters[0] is ItemScriptableObject item)
            {
                if (containerData && containerData.AcceptableItems.Contains(item))
                {
                    int amount = 1;
                    if (parameters[1] is int inAmount) amount = inAmount;
                    interactionInfo.OutArguments = new object[]
                    {
                        AddItem(item, amount)
                    };
                }
            }
            else if (parameters[0] is bool bClearItems) ClearItems(); // This will be used for stuff like harvesting
            else
            {
#if UNITY_EDITOR
                if (parameters[0] is not ItemScriptableObject && parameters[0] is not bool) Debug.LogWarning($"WARNING: Parameter 0 needs to be an ItemScriptableObject or a bool. Received {parameters[0]} type {parameters[0].GetType()} as parameter 0");
#endif
            }
        }
        return interactionInfo;
    }

    public AddItemInfo AddItem(ItemScriptableObject item, int amount = 1)
    {
        if (containerData == null || currentCapacity >= containerData.ContainerCapacity) return new AddItemInfo()
        {
            WasAdded = false
        };
        AddItemInfo info = ExtraAddLogic(item);
        if (info.WasAdded)
        {
            int adjustedAmount = amount;
            if (currentCapacity + amount > containerData.ContainerCapacity) adjustedAmount = containerData.ContainerCapacity - currentCapacity;
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
        }
        return info;
    }

    public void ClearItems()
    {
        ExtraClearLogic();
        acceptedItems.Clear();
        currentCapacity = 0;
    }

    protected abstract AddItemInfo ExtraAddLogic(ItemScriptableObject item);

    protected abstract void ExtraClearLogic();
}
