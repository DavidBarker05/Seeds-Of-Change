public struct InteractionInfo
{
    public bool EndInteraction { get; set; }
    public object[] OutArguments { get; set; }
}

public abstract class Interactable : UnityEngine.MonoBehaviour { public abstract InteractionInfo Interact(params object[] parameters); }