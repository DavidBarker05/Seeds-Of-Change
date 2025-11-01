using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MarketMenu : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI availableBalanceText;
    [SerializeField]
    PurchasableItemSlot purchasableItemSlotPrefab;
    [SerializeField]
    RectTransform purchasableContent;
    [SerializeField]
    SellableItemSlot sellableItemSlotPrefab;
    [SerializeField]
    RectTransform sellableContent;
    [SerializeField]
    GameObject purchaseMenu;
    [SerializeField]
    GameObject sellMenu;

    int availableBalance;
    List<ItemScriptableObject> purchasableItems;
    List<PurchasableItemSlot> purchasableItemSlots = new List<PurchasableItemSlot>();
    Dictionary<ItemScriptableObject, int> sellableItems = new Dictionary<ItemScriptableObject, int>();
    Dictionary<ItemScriptableObject, SellableItemSlot> sellableItemSlots = new Dictionary<ItemScriptableObject, SellableItemSlot>();

    void OnEnable()
    {
        purchaseMenu.SetActive(true);
        sellMenu.SetActive(false);
    }

    void CreateSellableItemSlot(ItemScriptableObject item, int amount)
    {
        if (sellableItemSlots != null && sellableItemSlots.ContainsKey(item)) return;
        SellableItemSlot slot = Instantiate(sellableItemSlotPrefab, sellableContent);
        slot.SetData(item, amount);
        slot.gameObject.SetActive(true);
        sellableItemSlots.Add(item, slot);
    }

    public void SetAvailableBalance(int money)
    {
        availableBalance = money;
        if (availableBalanceText != null) availableBalanceText.text = $"AVAILABLE BALANCE: {availableBalance}";
    }

    public void PopulatePurchaseMenu(List<ItemScriptableObject> itemList)
    {
        purchasableItems = new List<ItemScriptableObject>(itemList);
        for (int i = purchasableContent.childCount - 1; i >= 0; --i)
        {
            Destroy(purchasableContent.GetChild(i).gameObject);
        }
        sellableItemSlots.Clear();
        foreach (ItemScriptableObject item in purchasableItems)
        {
            PurchasableItemSlot slot = Instantiate(purchasableItemSlotPrefab, purchasableContent);
            slot.SetData(item);
            slot.gameObject.SetActive(true);
            purchasableItemSlots.Add(slot);
        }
    }

    public void UpdatePlayerInventory(ItemScriptableObject item, int amount)
    {
        if (!item.IsSellable) return;
        if (sellableItems.ContainsKey(item))
        {
            sellableItems[item] += amount;
            if (sellableItems[item] <= 0)
            {
                sellableItems.Remove(item);
                Destroy(sellableItemSlots[item].gameObject);
                sellableItemSlots.Remove(item);
            }
            else sellableItemSlots[item].SetData(item, sellableItems[item]);
        }
        else if (amount > 0)
        {
            sellableItems.Add(item, amount);
            CreateSellableItemSlot(item, amount);
        }
    }
}
