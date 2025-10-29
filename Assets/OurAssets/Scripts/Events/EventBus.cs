using UnityEngine;
using System.Collections.Generic;

public enum GameEventType
{
    None,
    NewDayEvent,
    ClearSkyWeatherEvent,
    RainWeatherEvent,
    DroughtDisasterEventStart,
    DroughtDisasterEventEnd
}

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }
    Dictionary<GameEventType, List<IEventListener>> eventListeners = new Dictionary<GameEventType, List<IEventListener>>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void AddEventListener(GameEventType eventType, IEventListener eventListener)
    {
        if (eventListeners.ContainsKey(eventType))
        {
            if (!eventListeners[eventType].Contains(eventListener)) eventListeners[eventType].Add(eventListener);
        }
        else
        {
            eventListeners.Add(eventType, new List<IEventListener>());
            eventListeners[eventType].Add(eventListener);
        }
    }

    public void RemoveEventListener(GameEventType eventType, IEventListener eventListener)
    {
        if (!eventListeners.ContainsKey(eventType) || !eventListeners[eventType].Contains(eventListener)) return;
        eventListeners[eventType].Remove(eventListener);
        if (eventListeners[eventType].Count <= 0) eventListeners.Remove(eventType);
    }

    public void BroadcastEvent(GameEventType eventType, params object[] parameters)
    {
        if (!eventListeners.ContainsKey(eventType)) return;
        for (int i = eventListeners[eventType].Count - 1; i >= 0; --i)
        {
            eventListeners[eventType][i].OnEventReceived(eventType, parameters);
        }
    }
}
