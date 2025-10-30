using UnityEngine;

[CreateAssetMenu(fileName = "PhysicalItemScriptableObject", menuName = "Scriptable Objects/PhysicalItemScriptableObject")]
public class PhysicalItemScriptableObject : ItemScriptableObject
{
    [SerializeField]
    GameObject model;

    public GameObject Model => model;
}
