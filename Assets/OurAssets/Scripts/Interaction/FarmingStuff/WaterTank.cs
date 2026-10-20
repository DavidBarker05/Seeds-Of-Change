using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WaterTank : Interactable, IEventListener
{
    [SerializeField, FormerlySerializedAs("waterItem")]
    ItemScriptableObject m_WaterItem;
    [SerializeField, Min(0f), FormerlySerializedAs("maxCapacity")]
    float m_MaxCapacity = 10f;
    [SerializeField, Min(0f), FormerlySerializedAs("startingCapacity")]
    float m_StartingCapacity = 1f;
    [SerializeField, Min(0f), FormerlySerializedAs("waterGainedFromPurchasing")]
    float m_WaterGainedFromPurchasing = 2.5f;

    private static readonly Dictionary<Difficulty, float> s_WaterGainedWhileRaining = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Easy, 1.5f },
        { Difficulty.Normal, 1f },
        { Difficulty.Hard, 0.75f }
    };

    public float CurrentCapacity { get; private set; }

    void Awake()
    {
        CurrentCapacity = m_StartingCapacity;
        EventBus.Instance?.AddEventListener("RainWeatherEvent", this);
        EventBus.Instance?.AddEventListener("ItemPurchasedEvent", this);
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener("RainWeatherEvent", this);
        EventBus.Instance?.RemoveEventListener("ItemPurchasedEvent", this);
    }

    void AddWater(float amount)
    {
        if (CurrentCapacity != m_MaxCapacity) CurrentCapacity = Mathf.Clamp(CurrentCapacity + amount, 0f, m_MaxCapacity);
    }

    public override InteractionInfo Interact(params object[] parameters)
    {
        if (parameters.Length != 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: WaterTank objects needs 1 parameter. Received {parameters.Length} parameters");
#endif
        }
        else if (parameters[0] is WateringCan wateringCan)
        {
            if (wateringCan.CurrentCapacity != wateringCan.MaximumCapacity)
            {
                if (CurrentCapacity <= wateringCan.MaximumCapacity - wateringCan.CurrentCapacity)
                {
                    wateringCan.CurrentCapacity += CurrentCapacity;
                    CurrentCapacity = 0f;
                }
                else
                {
                    CurrentCapacity -= wateringCan.MaximumCapacity - wateringCan.CurrentCapacity;
                    wateringCan.CurrentCapacity = wateringCan.MaximumCapacity;
                }
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: Parameter 0 needs to be a WateringCan. Received {parameters[0]} type {parameters[0].GetType()} as parameter 0");
#endif
        }
        return new InteractionInfo() { EndInteraction = true };
    }

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        switch (eventName)
        {
            case "RainWeatherEvent":
                AddWater(s_WaterGainedWhileRaining[WorldManager.Instance?.CurrentDifficulty ?? Difficulty.Normal]);
                break;
            case "ItemPurchasedEvent":
                if (parameters[0] is ItemScriptableObject item && item == m_WaterItem && parameters[1] is int amount) AddWater(m_WaterGainedFromPurchasing * amount);
                break;
            default:
                break;
        }
    }
}
