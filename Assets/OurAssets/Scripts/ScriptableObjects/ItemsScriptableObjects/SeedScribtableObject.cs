using UnityEngine;

[CreateAssetMenu(fileName = "SeedScribtableObject", menuName = "Scriptable Objects/SeedScribtableObject")]
public class SeedScribtableObject : ItemScriptableObject
{
    [SerializeField]
    GameObject packetModel;
    [SerializeField]
    CropScriptableObject cropToPlant;
    [SerializeField]
    int numberOfUses;

    public GameObject PacketModel => packetModel;
    public CropScriptableObject CropToPlant => cropToPlant;
    public int NumberOfUses => numberOfUses;
}
