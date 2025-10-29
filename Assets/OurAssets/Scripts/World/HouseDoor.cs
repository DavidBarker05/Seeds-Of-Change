using UnityEngine;

public class HouseDoor : Interactable
{
    public override bool Interact(params object[] parameters)
    {
        if (parameters.Length != 1)
        {
            #if UNITY_EDITOR
                Debug.LogWarning($"WARNING: HouseDoor needs 1 parameters. Received {parameters.Length} parameters");
            #endif

        }
        else
        {
            if (parameters[0] is GameObject player)
            {
                SpawnManager spawnManager = player.GetComponent<SpawnManager>();
                if (spawnManager != null)
                {
                    spawnManager.Respawn();
                    WorldManager.Instance?.MoveToNextDay();
                }
                else
                {
                    #if UNITY_EDITOR
                        Debug.LogWarning($"WARNING: Parameter 0 needs to be a game object with a SpawnManager script attached");
                    #endif
                }
            }
            else
            {
                #if UNITY_EDITOR
                    if (parameters[0] is not GameObject) Debug.LogWarning($"WARNING: Parameter 0 needs to be a game object. Received {parameters[0]} type {parameters[0].GetType()} as parameter 0");
                #endif
            }
        }
        return true;
    }
}
