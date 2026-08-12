// Used to make pest not block any of the plants or farm tiles
public class PestScript : Interactable
{
    public override InteractionInfo Interact(params object[] parameters) => new InteractionInfo() { DoEndInteraction = true };
}