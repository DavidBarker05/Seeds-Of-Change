using UnityEngine;

public class BTSpray : Usable
{

    [SerializeField, Min(0)]
    int numberOfUses = 10;

    int currentNumberOfUses;

    new void Awake()
    {
        base.Awake();
        currentNumberOfUses = numberOfUses;
    }

    public override bool Use(Interactable targetInteractable)
    {
        if (targetInteractable is FarmTile farmTile)
        {
            farmTile.ApplyActivePesticide();
            --currentNumberOfUses;
        }
        else if (targetInteractable is CropScript crop)
        {
            EventBus.Instance?.BroadcastEvent(GameEventType.CropActivePestAppliedEvent, crop.gameObject);
            --currentNumberOfUses;
        }
        else if (targetInteractable is PestScript pest)
        {
            EventBus.Instance?.BroadcastEvent(GameEventType.PestActivePestAppliedEvent, pest.gameObject);
            --currentNumberOfUses;
        }
        return currentNumberOfUses <= 0;
    }
}
