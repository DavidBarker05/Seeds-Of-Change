using UnityEngine;

[CreateAssetMenu(fileName = "PhysicalItemScriptableObject", menuName = "Scriptable Objects/PhysicalItemScriptableObject")]
public class PhysicalItemScriptableObject : ItemScriptableObject
{
    [field: Header("Model Data")]
    [field: SerializeField]
    public GameObject Model { get; private set; }
}
