using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WetObject : MonoBehaviour, IEventListener
{
    Renderer m_Renderer;

    private void Awake()
    {
        m_Renderer = GetComponent<Renderer>();
        EventBus.Instance?.AddEventListener("ClearSkyWeatherEvent", this);
        EventBus.Instance?.AddEventListener("RainWeatherEvent", this);
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener("ClearSkyWeatherEvent", this);
        EventBus.Instance?.RemoveEventListener("RainWeatherEvent", this);
    }

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        if (eventName == "ClearSkyWeatherEvent") m_Renderer.material.SetInt("_IsWet", 0);
        else if (eventName == "RainWeatherEvent") m_Renderer.material.SetInt("_IsWet", 1);
    }
}
