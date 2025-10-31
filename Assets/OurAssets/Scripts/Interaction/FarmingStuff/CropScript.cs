// Attach this to the crop prefabs, used so that watering isn't as hard
public class CropScript : Interactable { public override bool Interact(params object[] parameters) => true; }