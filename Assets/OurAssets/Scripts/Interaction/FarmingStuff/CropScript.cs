// Attach this to the crop prefabs
public class CropScript : Interactable
{
    public override InteractionInfo Interact(params object[] parameters)
    {
        if (parameters.Length == 0) EventBus.Instance?.BroadcastEvent(GameEventType.CropHarvestedEvent, gameObject); // Only if not holding anything
        return new InteractionInfo() { DoEndInteraction = true }; // End interaction no matter what
    }
}