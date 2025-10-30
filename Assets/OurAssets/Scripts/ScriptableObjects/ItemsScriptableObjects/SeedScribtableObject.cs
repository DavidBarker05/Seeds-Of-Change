using UnityEngine;

[CreateAssetMenu(fileName = "SeedScribtableObject", menuName = "Scriptable Objects/SeedScribtableObject")]
public class SeedScribtableObject : PhysicalItemScriptableObject
{
    [SerializeField]
    CropScriptableObject cropToPlant;
    [SerializeField]
    int numberOfUses;

    public CropScriptableObject CropToPlant => cropToPlant;
    public int NumberOfUses => numberOfUses;
}
