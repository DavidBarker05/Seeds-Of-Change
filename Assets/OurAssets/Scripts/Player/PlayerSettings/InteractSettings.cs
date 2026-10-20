using UnityEngine;

[CreateAssetMenu(fileName = "InteractSettings", menuName = "Player/Interact Settings")]
public class InteractSettings : ScriptableObject
{
    [field: SerializeField]
    public LayerMask InteractableLayer { get; private set; }
    [field: SerializeField]
    public float InteractionDistance { get; private set; } = 2f;
    [field: SerializeField, Range(0.1f, 0.5f)]
    public float InteractionRadius { get; private set; } = 0.1f;
    [field: SerializeField]
    public LayerMask HoldLayer { get; private set; }
}
