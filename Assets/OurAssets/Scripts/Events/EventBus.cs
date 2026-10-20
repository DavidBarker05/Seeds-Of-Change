using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }

    readonly Dictionary<string, List<IEventListener>> m_EventListeners = new Dictionary<string, List<IEventListener>>();

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void AddEventListener(string eventName, IEventListener eventListener)
    {
        if (m_EventListeners.ContainsKey(eventName))
        {
            if (!m_EventListeners[eventName].Contains(eventListener)) m_EventListeners[eventName].Add(eventListener);
        }
        else
        {
            m_EventListeners.Add(eventName, new List<IEventListener>());
            m_EventListeners[eventName].Add(eventListener);
        }
    }

    public void RemoveEventListener(string eventName, IEventListener eventListener)
    {
        if (!m_EventListeners.ContainsKey(eventName) || !m_EventListeners[key: eventName].Contains(eventListener)) return;
        m_EventListeners[eventName].Remove(eventListener);
        if (m_EventListeners[eventName].Count <= 0) m_EventListeners.Remove(eventName);
    }

    public void BroadcastEvent(string eventName, params object[] parameters)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        if (!m_EventListeners.ContainsKey(eventName)) return;
        for (int i = m_EventListeners[eventName].Count - 1; i >= 0; --i)
        {
            m_EventListeners[eventName][i].OnEventReceived(eventName, parameters);
        }
    }
}
