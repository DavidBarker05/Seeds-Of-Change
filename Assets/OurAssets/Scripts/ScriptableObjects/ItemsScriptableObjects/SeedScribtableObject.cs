using UnityEngine;

[CreateAssetMenu(fileName = "SeedScribtableObject", menuName = "Scriptable Objects/SeedScribtableObject")]
public class SeedScribtableObject : PhysicalItemScriptableObject
{
    [field: Header("Seed Data")]
    [field: SerializeField]
    public CropScriptableObject CropToPlant { get; private set; }
    [field: SerializeField]
    public int NumberOfUses { get; private set; }
}
