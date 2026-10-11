using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WetObject : MonoBehaviour, IEventListener
{
    Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        EventBus.Instance?.AddEventListener(GameEventType.ClearSkyWeatherEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.RainWeatherEvent, this);
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.ClearSkyWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.RainWeatherEvent, this);
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.ClearSkyWeatherEvent:
                _renderer.material.SetInt("_IsWet", 0);
                break;
            case GameEventType.RainWeatherEvent:
                _renderer.material.SetInt("_IsWet", 1);
                break;
            default:
                break;
        }
    }
}
