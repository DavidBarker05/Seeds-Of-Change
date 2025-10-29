using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemContainerScriptableObject", menuName = "Scriptable Objects/ItemContainerScriptableObject")]
public class ItemContainerScriptableObject : ScriptableObject
{
    [SerializeField]
    List<ItemScriptableObject> acceptableItems = new List<ItemScriptableObject>();
    [SerializeField]
    int containerCapacity;

    public List<ItemScriptableObject> AcceptableItems => new List<ItemScriptableObject>(acceptableItems);
    public int ContainerCapacity => containerCapacity;
}
