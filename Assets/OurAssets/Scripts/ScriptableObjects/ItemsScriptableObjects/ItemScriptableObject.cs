using UnityEngine;

[CreateAssetMenu(fileName = "ItemScriptableObject", menuName = "Scriptable Objects/ItemScriptableObject")]
public class ItemScriptableObject : ScriptableObject
{
    [SerializeField]
    string itemName;
    [SerializeField]
    bool isPurchasable;
    [SerializeField]
    bool isSellable;
    [SerializeField, Tooltip("Only will be used if the item is purchasable")]
    float purchasePrice;
    [SerializeField, Tooltip("Only will be used if the item is sellable")]
    float sellPrice;

    public string ItemName => itemName;
    public bool IsPurchasable => isPurchasable;
    public bool IsSellable => isSellable;
    public float PurchasePrice => isPurchasable ? purchasePrice : -1f;
    public float SellPrice => isSellable ? sellPrice : -1f;
}
