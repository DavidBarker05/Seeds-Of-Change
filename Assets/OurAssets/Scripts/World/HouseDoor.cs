using UnityEngine;

public class HouseDoor : Interactable
{
    public override InteractionInfo Interact(params object[] parameters)
    {
        if (parameters.Length != 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: HouseDoor needs 1 parameters. Received {parameters.Length} parameters");
#endif
        }
        else
        {
            if (parameters[0] is FirstPersonPlayerCharacter player)
            {
                player.Respawn();
                WorldManager.Instance?.MoveToNextDay();
            }
            else
            {
#if UNITY_EDITOR
                if (parameters[0] is not GameObject) Debug.LogWarning($"WARNING: Parameter 0 needs to be First Person Player Character. Received {parameters[0]} type {parameters[0].GetType()} as parameter 0");
#endif
            }
        }
        return new InteractionInfo() { EndInteraction = true };
    }
}
