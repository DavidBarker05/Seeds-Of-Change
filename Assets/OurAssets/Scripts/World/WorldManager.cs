using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum Season
{
    RainySeason,
    CoolSeason,
    HotSeason,
    DrySeason
}

public enum Weather
{
    None,
    ClearSky,
    Rain
}

public enum Disaster
{
    None,
    Drought
}

public class WorldManager : MonoBehaviour, IEventListener
{
    public static WorldManager Instance { get; private set; }

    [SerializeField, Min(1)]
    int daysPerSeason = 10;
    [SerializeField]
    Season startingSeason = Season.RainySeason;


    SeasonManager seasonManager;
    WeatherManager weatherManager;
    DisasterManager disasterManager;
    

    public int CurrentDay { get; private set; }
    public int CurrentDayInSeason { get; private set; }
    public Season CurrentSeason => seasonManager.CurrentSeason;
    public Weather CurrentWeather => weatherManager.CurrentWeather;

    Weather forcedWeather = Weather.None;
    Disaster forcedDisaster = Disaster.None;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        seasonManager = new SeasonManager(startingSeason);
        weatherManager = new WeatherManager();
        disasterManager = new DisasterManager();
    }

    void Start()
    {
        CurrentDay = 1;
        CurrentDayInSeason = 1;
        weatherManager.DoWeather(CurrentSeason);
    }

    public void MoveToNextDay()
    {
        ++CurrentDay;
        ++CurrentDayInSeason;
        if (CurrentDayInSeason > daysPerSeason)
        {
            seasonManager.DoSeasonChange();
            CurrentDayInSeason = 1;
        }
        if (forcedWeather == Weather.None) weatherManager.DoWeather(CurrentSeason);
        else
        {
            weatherManager.DoWeather(forcedWeather);
            forcedWeather = Weather.None;
        }
        if (forcedDisaster == Disaster.None) disasterManager.DoDisaster(CurrentSeason);
        else
        {
            disasterManager.DoDisaster(forcedDisaster);
            forcedDisaster = Disaster.None;
        }
        EventBus.Instance?.BroadcastEvent(GameEventType.NewDayEvent);
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.DroughtDisasterEventEnd:
                forcedWeather = Weather.Rain;
                break;
            default:
                break;
        }
    }

    private class SeasonManager
    {
        public Season CurrentSeason { get; private set; }

        public SeasonManager(Season startingSeason) => CurrentSeason = startingSeason;

        public void DoSeasonChange() => CurrentSeason = (Season)(((int)CurrentSeason + 1) % System.Enum.GetValues(typeof(Season)).Length);
    }

    private class WeatherManager
    {
        public Weather CurrentWeather { get; private set; }

        public WeatherManager() => CurrentWeather = Weather.ClearSky;

        public void DoWeather(Weather weather)
        {
            CurrentWeather = weather;
            GameEventType eventType = CurrentWeather switch
            {
                Weather.ClearSky => GameEventType.ClearSkyWeatherEvent,
                Weather.Rain => GameEventType.RainWeatherEvent,
                _ => GameEventType.None
            };
            EventBus.Instance?.BroadcastEvent(eventType);
        }

        public void DoWeather(Season currentSeason)
        {
            DoWeather(Weather.Rain);
        }
    }

    private class DisasterManager
    {
        Dictionary<Disaster, int> activeDisasters = new Dictionary<Disaster, int>();

        public void DoDisaster(Disaster disaster)
        {
            TickDisasters();
        }

        public void DoDisaster(Season currentSeason)
        {
            DoDisaster(Disaster.None);
        }

        void TickDisasters()
        {
            List<Disaster> disastersToDelete = new List<Disaster>();
            foreach (Disaster activeDisaster in activeDisasters.Keys)
            {
                --activeDisasters[activeDisaster];
                if (activeDisasters[activeDisaster] <= 0) disastersToDelete.Add(activeDisaster);
            }
            foreach (Disaster disaster in disastersToDelete)
            {
                BroadcastDisasterEnded(disaster);
                activeDisasters.Remove(disaster);
            }
        }

        void BroadcastDisasterStarted(Disaster disaster)
        {
            GameEventType eventType = disaster switch
            {
                Disaster.Drought => GameEventType.DroughtDisasterEventStart,
                _ => GameEventType.None,
            };
            EventBus.Instance?.BroadcastEvent(eventType);
        }

        void BroadcastDisasterEnded(Disaster disaster)
        {
            GameEventType eventType = disaster switch
            {
                Disaster.Drought => GameEventType.DroughtDisasterEventEnd,
                _ => GameEventType.None,
            };
            EventBus.Instance?.BroadcastEvent(eventType);
        }
    }
}
