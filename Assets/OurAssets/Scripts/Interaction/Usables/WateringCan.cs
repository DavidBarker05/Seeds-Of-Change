using UnityEngine;
using UnityEngine.Serialization;

public class WateringCan : Usable
{
    [SerializeField, FormerlySerializedAs("maximumCapacity")]
    float m_MaximumCapacity = 1f;
    [SerializeField, FormerlySerializedAs("waterLostWhenWatering")]
    float m_WaterLostWhenWatering = 0.1f;

    public float MaximumCapacity => m_MaximumCapacity;
    public float CurrentCapacity { get; set; }

    public override UseInfo Use(Interactable targetInteractable)
    {
        if (CurrentCapacity >= m_WaterLostWhenWatering)
        {
            if (targetInteractable is FarmTile farmTile)
            {
                farmTile.WaterPlant();
                CurrentCapacity -= m_WaterLostWhenWatering;
            }
            else if (targetInteractable is CropScript crop)
            {
                EventBus.Instance?.BroadcastEvent("PlantWateredEvent", crop.gameObject);
                CurrentCapacity -= m_WaterLostWhenWatering;
            }
            else if (targetInteractable is PestScript pest)
            {
                EventBus.Instance?.BroadcastEvent("PlantWateredEvent", pest.gameObject);
                CurrentCapacity -= m_WaterLostWhenWatering;
            }
        }
        return new UseInfo()
        {
            FinishedUsing = false // Never delete
        };
    }
}
