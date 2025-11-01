// Used to make pest not block any of the plants or farm tiles
public class PestScript : Interactable { public override bool Interact(params object[] parameters) => true; }