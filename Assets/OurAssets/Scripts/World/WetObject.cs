using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WetObject : MonoBehaviour, IEventListener
{
    [SerializeField]
    Material dryMaterial;
    [SerializeField]
    Material wetMaterial;

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
                _renderer.material = dryMaterial;
                break;
            case GameEventType.RainWeatherEvent:
                _renderer.material = wetMaterial;
                break;
            default:
                break;
        }
    }
}
