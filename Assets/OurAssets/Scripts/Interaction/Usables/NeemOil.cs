using UnityEngine;

public class NeemOil : Usable
{
    [SerializeField, Min(0)]
    int numberOfUses = 10;
    [SerializeField, Min(0f)]
    float percentReduction = 50f;
    [SerializeField, Min(0)]
    int duration = 5;

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
            farmTile.ApplyPassivePesticide(percentReduction, duration);
            --currentNumberOfUses;
        }
        else if (targetInteractable is CropScript crop)
        {
            EventBus.Instance?.BroadcastEvent(GameEventType.CropPassivePestAppliedEvent, crop.gameObject, percentReduction, duration);
            --currentNumberOfUses;
        }
        else if (targetInteractable is PestScript pest)
        {
            EventBus.Instance?.BroadcastEvent(GameEventType.PestPassivePestAppliedEvent, pest.gameObject, percentReduction, duration);
            --currentNumberOfUses;
        }
        return currentNumberOfUses <= 0;
    }
}
