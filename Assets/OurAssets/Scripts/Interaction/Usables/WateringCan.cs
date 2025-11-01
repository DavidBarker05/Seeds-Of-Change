using UnityEngine;

public class WateringCan : Usable
{
    [SerializeField]
    float maximumCapacity = 1f;
    [SerializeField]
    float waterLostWhenWatering = 0.1f;

    public float MaximumCapacity => maximumCapacity;
    public float CurrentCapacity { get; set; }

    public override bool Use(Interactable targetInteractable)
    {
        if (CurrentCapacity >= waterLostWhenWatering)
        {
            if (targetInteractable is FarmTile farmTile)
            {
                farmTile.WaterPlant();
                CurrentCapacity -= waterLostWhenWatering;
            }
            else if (targetInteractable is CropScript crop)
            {
                EventBus.Instance?.BroadcastEvent(GameEventType.PlantWateredEvent, crop.gameObject);
                CurrentCapacity -= waterLostWhenWatering;
            }
            else if (targetInteractable is PestScript pest)
            {
                EventBus.Instance?.BroadcastEvent(GameEventType.PlantWateredEvent, pest.gameObject);
                CurrentCapacity -= waterLostWhenWatering;
            }
        }
        return false; // Never delete
    }
}
