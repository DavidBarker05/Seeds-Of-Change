using UnityEngine;

[CreateAssetMenu(fileName = "ItemScriptableObject", menuName = "Scriptable Objects/ItemScriptableObject")]
public class ItemScriptableObject : ScriptableObject
{
    [field: Header("Item Data")]
    [field: SerializeField]
    public string ItemName { get; private set; }
    [field: SerializeField]
    public bool IsPurchasable { get; private set; }
    [field: SerializeField]
    public bool IsSellable { get; private set; }
    [field: SerializeField, Tooltip("Only will be used if the item is purchasable")]
    public float PurchasePrice { get; private set; }
    [field: SerializeField, Tooltip("Only will be used if the item is sellable")]
    public float SellPrice { get; private set; }
}
