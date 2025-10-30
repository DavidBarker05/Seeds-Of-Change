using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemContainerScriptableObject", menuName = "Scriptable Objects/ItemContainerScriptableObject")]
public class ItemContainerScriptableObject : ScriptableObject
{
    [field: SerializeField]
    public string ContainerName { get; set; }
    [field: SerializeField]
    public List<ItemScriptableObject> AcceptableItems { get; private set; }
    [field: SerializeField]
    public int ContainerCapacity { get; private set; }
}
