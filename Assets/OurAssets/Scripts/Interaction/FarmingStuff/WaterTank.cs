using UnityEngine;
using System.Collections.Generic;

public class WaterTank : Interactable, IEventListener
{
    [SerializeField]
    ItemScriptableObject waterItem;
    [SerializeField, Min(0f)]
    float maxCapacity = 10f;
    [SerializeField, Min(0f)]
    float startingCapacity = 1f;
    [SerializeField, Min(0f)]
    float waterGainedFromPurchasing = 2.5f;

    private static readonly Dictionary<Difficulty, float> waterGainedWhileRaining = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Easy, 1.5f },
        { Difficulty.Normal, 1f },
        { Difficulty.Hard, 0.75f }
    };

    public float CurrentCapacity { get; private set; }

    void Awake()
    {
        CurrentCapacity = startingCapacity;
        EventBus.Instance?.AddEventListener(GameEventType.RainWeatherEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.ItemPurchasedEvent, this);
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.RainWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.ItemPurchasedEvent, this);
    }

    void AddWater(float amount)
    {
        if (CurrentCapacity != maxCapacity) CurrentCapacity = Mathf.Clamp(CurrentCapacity + amount, 0f, maxCapacity);
    }

    public override bool Interact(params object[] parameters)
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
        return true;
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.RainWeatherEvent:
                AddWater(waterGainedWhileRaining[WorldManager.Instance?.CurrentDifficulty ?? Difficulty.Normal]);
                break;
            case GameEventType.ItemPurchasedEvent:
                if (parameters[0] is ItemScriptableObject item && item == waterItem && parameters[1] is int amount) AddWater(waterGainedFromPurchasing * amount);
                break;
            default:
                break;
        }
    }
}
