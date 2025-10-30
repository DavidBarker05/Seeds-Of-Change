using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public enum Season
{
    RainySeason,
    CoolSeason,
    HotSeason,
    DrySeason
}

[System.Serializable]
public enum Weather
{
    None,
    ClearSky,
    Rain
}

public enum Disaster
{
    None,
    Drought,
    Pest
}

[System.Serializable]
public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class WorldManager : MonoBehaviour, IEventListener
{
    public static WorldManager Instance { get; private set; }

    [SerializeField, Min(1)]
    int daysPerSeason = 10;
    [SerializeField]
    Season startingSeason = Season.RainySeason;
    [SerializeField]
    Weather startingWeather = Weather.ClearSky;
    [SerializeField]
    Difficulty startingDifficulty = Difficulty.Easy;

    SeasonManager seasonManager;
    WeatherManager weatherManager;
    DisasterManager disasterManager;

    public int CurrentDay { get; private set; }
    public int CurrentDayInSeason { get; private set; }
    public int CurrentYear { get; private set; }
    public Season CurrentSeason => seasonManager.CurrentSeason;
    public Weather CurrentWeather => weatherManager.CurrentWeather;
    public Difficulty CurrentDifficulty { get; private set; }

    bool canResetForcedWeather;
    Weather forcedWeather = Weather.None;
    bool canResetForcedDisaster;
    Disaster forcedDisaster = Disaster.None;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        seasonManager = new SeasonManager(startingSeason);
        weatherManager = new WeatherManager();
        disasterManager = new DisasterManager();
        CurrentDifficulty = startingDifficulty;
    }

    void Start()
    {
        EventBus.Instance?.AddEventListener(GameEventType.DroughtDisasterEventStart, this);
        EventBus.Instance?.AddEventListener(GameEventType.DroughtDisasterEventEnd, this);
        canResetForcedWeather = true;
        canResetForcedDisaster = true;
        CurrentDay = 1;
        CurrentDayInSeason = 1;
        CurrentYear = 1;
        forcedWeather = startingWeather;
        DoWeather();
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventStart, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventEnd, this);
    }

    public void MoveToNextDay()
    {
        ++CurrentDay;
        ++CurrentDayInSeason;
        if (CurrentDayInSeason > daysPerSeason) DoSeasonChange();
        EventBus.Instance?.BroadcastEvent(GameEventType.NewDayEvent);
        DoDisaster();
        DoWeather();
    }

    void DoSeasonChange()
    {
        seasonManager.DoSeasonChange(); // Change the season to next season
        CurrentDayInSeason = 1; // Now first day in new season
        if (CurrentSeason == startingSeason)
        {
            ++CurrentYear; // Back to first season so new year
            if (CurrentYear >= 2) // Increase difficulty every year
            {
                if (CurrentDifficulty == Difficulty.Easy) CurrentDifficulty = Difficulty.Normal;
                else if (CurrentDifficulty == Difficulty.Normal) CurrentDifficulty = Difficulty.Hard;
            }
        }
    }

    void DoWeather()
    {
        if (forcedWeather == Weather.None) weatherManager.DoWeather(CurrentSeason, CurrentDifficulty);
        else
        {
            weatherManager.DoWeather(forcedWeather, CurrentDifficulty);
            if (canResetForcedWeather) forcedWeather = Weather.None;
        }
    }

    void DoDisaster()
    {
        disasterManager.TickDisasters(CurrentDifficulty);
        if (forcedDisaster == Disaster.None) disasterManager.DoDisaster(CurrentSeason, CurrentDifficulty);
        else
        {
            disasterManager.DoDisaster(forcedDisaster, CurrentDifficulty);
            if (canResetForcedDisaster) forcedDisaster = Disaster.None;
        }
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.DroughtDisasterEventStart:
                canResetForcedWeather = false;
                forcedWeather = Weather.ClearSky;
                break;
            case GameEventType.DroughtDisasterEventEnd:
                canResetForcedWeather = true;
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

        public void DoSeasonChange()
        {
            Season[] seasons = System.Enum.GetValues(typeof(Season)) as Season[];
            int nextSeasonIndex = (System.Array.IndexOf(seasons, CurrentSeason) + 1) % seasons.Length;
            CurrentSeason = seasons[nextSeasonIndex];
        }
    }

    private class WeatherManager
    {
        private static readonly Dictionary<Season, (float minChance, float maxChance)> baseRainChances = new Dictionary<Season, (float minChance, float maxChance)>()
        {
            { Season.RainySeason, (minChance: 0.45f, maxChance: 0.65f) },
            { Season.CoolSeason, (minChance: 0.25f, maxChance: 0.4f) },
            { Season.HotSeason, (minChance: 0.08f, maxChance: 0.15f) },
            { Season.DrySeason, (minChance: 0.03f, maxChance: 0.08f) }
        };

        private static readonly Dictionary<Difficulty, float> difficultyModifiers = new Dictionary<Difficulty, float>()
        {
            { Difficulty.Easy, 1f },
            { Difficulty.Normal, 0.9f },
            { Difficulty.Hard, 0.7f }
        };

        private static readonly Dictionary<Difficulty, (float minAccumulation, float maxAccumulation, float accumulationDecrease, float accumulationIncrease)> difficultyAccumulations = new Dictionary<Difficulty, (float minAccumulation, float maxAccumulation, float accumulationDecrease, float accumulationIncrease)>()
        {
            { Difficulty.Easy, (minAccumulation: -0.05f, maxAccumulation: 0.15f, accumulationDecrease: -0.005f, accumulationIncrease: 0.015f) },
            { Difficulty.Normal, (minAccumulation: -0.1f, maxAccumulation: 0.1f, accumulationDecrease: -0.01f, accumulationIncrease: 0.01f) },
            { Difficulty.Hard, (minAccumulation: -0.15f, maxAccumulation: 0.05f, accumulationDecrease: -0.015f, accumulationIncrease: 0.005f) }
        };

        private static readonly float DEVIATION = 0.1f; // 10%
        private static readonly float TOLERANCE = 0.001f; // Amount a percentage can stray from 0 or 1 to be considered 0 or 1

        private float currentAccumulation = 0f;

        public Weather CurrentWeather { get; private set; }

        public WeatherManager() => CurrentWeather = Weather.ClearSky;

        public void DoWeather(Weather weather, Difficulty currentDifficulty = Difficulty.Normal)
        {
            if (weather == Weather.ClearSky)
            {
                if (currentAccumulation < 0f) currentAccumulation = 0f; // Reset accumulation
                else
                {
                    float maxAccumulation = Mathf.Max(difficultyAccumulations[currentDifficulty].maxAccumulation, 0f); // Ensure max >= 0
                    float accumulationIncrease = Mathf.Max(difficultyAccumulations[currentDifficulty].accumulationIncrease, 0f); // Ensure increase >= 0
                    currentAccumulation = Mathf.Clamp(currentAccumulation + accumulationIncrease, 0f, maxAccumulation);
                }
            }
            else if (weather == Weather.Rain)
            {
                if (currentAccumulation > 0f) currentAccumulation = 0f; // Reset accumulation
                else
                {
                    float minAccumulation = Mathf.Min(difficultyAccumulations[currentDifficulty].minAccumulation, 0f); // Ensure min <= 0
                    float accumulationDecrease = Mathf.Min(difficultyAccumulations[currentDifficulty].accumulationDecrease, 0f); // Ensure decrease <= 0
                    currentAccumulation = Mathf.Clamp(currentAccumulation + accumulationDecrease, minAccumulation, 0f);
                }
            }
            CurrentWeather = weather;
            GameEventType eventType = CurrentWeather switch
            {
                Weather.ClearSky => GameEventType.ClearSkyWeatherEvent,
                Weather.Rain => GameEventType.RainWeatherEvent,
                _ => GameEventType.None
            };
            EventBus.Instance?.BroadcastEvent(eventType);
        }

        public void DoWeather(Season currentSeason, Difficulty currentDifficulty = Difficulty.Normal)
        {
            float minRainChance = Mathf.Clamp01(baseRainChances[currentSeason].minChance); // Ensures that if the value is ever modified it remains between 0 and 1
            float maxRainChance = Mathf.Clamp01(baseRainChances[currentSeason].maxChance); // Ensures that if the value is ever modified it remains between 0 and 1
            float baseRainChance = Random.Range(minRainChance, maxRainChance); // Base rain chance between 0 and 1 no multipliers or accumulation

            float deviation = Mathf.Max(Random.Range(1f - DEVIATION, 1f + DEVIATION), TOLERANCE); // Ensure the value > 0 for multiplication
            float difficultyModifier = Mathf.Max(difficultyModifiers[currentDifficulty], TOLERANCE); // Ensure the value > 0 for multiplication

            float totalRainChance = Mathf.Clamp01(baseRainChance * deviation * difficultyModifier + currentAccumulation); // Total chance with multipliers and accumulation between 0 and 1

            if (totalRainChance <= TOLERANCE) DoWeather(Weather.ClearSky, currentDifficulty); // ~0% chance do clear sky
            else if (totalRainChance >= 1f - TOLERANCE) DoWeather(Weather.Rain, currentDifficulty); // ~100% do rain
            else DoWeather(Random.value > totalRainChance ? Weather.ClearSky : Weather.Rain, currentDifficulty); // Random weather
        }
    }

    private class DisasterManager
    {
        private static readonly Dictionary<Season, Dictionary<Disaster, (float minChance, float maxChance)>> baseDisasterChances = new Dictionary<Season, Dictionary<Disaster, (float minChance, float maxChance)>>()
        {
            {
                Season.RainySeason,
                new Dictionary<Disaster, (float minChance , float maxChance)>()
                {
                    { Disaster.Drought, (minChance: 0.01f, maxChance: 0.05f) },
                    { Disaster.Pest, (minChance: 0.2f, maxChance: 0.3f) }
                }
            },
            {
                Season.CoolSeason,
                new Dictionary<Disaster, (float minChance , float maxChance)>()
                {
                    { Disaster.Drought, (minChance: 0.1f, maxChance: 0.2f) },
                    { Disaster.Pest, (minChance: 0.1f, maxChance: 0.15f) }
                }
            },
            {
                Season.HotSeason,
                new Dictionary<Disaster, (float minChance , float maxChance)>()
                {
                    { Disaster.Drought, (minChance: 0.25f, maxChance: 0.3f) },
                    { Disaster.Pest, (minChance: 0.15f, maxChance: 0.25f) }
                }
            },
            {
                Season.DrySeason,
                new Dictionary<Disaster, (float minChance , float maxChance)>()
                {
                    { Disaster.Drought, (minChance: 0.4f, maxChance: 0.6f) },
                    { Disaster.Pest, (minChance: 0.05f, maxChance: 0.1f) }
                }
            }
        };

        private static readonly Dictionary<Season, List<Disaster>> disasterPriorities = new Dictionary<Season, List<Disaster>>()
        {
            {
                Season.RainySeason,
                new List<Disaster>()
                {
                    Disaster.Pest,
                    Disaster.Drought
                }
            },
            {
                Season.CoolSeason,
                new List<Disaster>()
                {
                    Disaster.Pest,
                    Disaster.Drought
                }
            },
            {
                Season.HotSeason,
                new List<Disaster>()
                {
                    Disaster.Drought,
                    Disaster.Pest
                }
            },
            {
                Season.DrySeason,
                new List<Disaster>()
                {
                    Disaster.Drought,
                    Disaster.Pest
                }
            }
        };

        private static readonly Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>> disasterDurations = new Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>>()
        {
            {
                Disaster.Drought,
                new Dictionary<Difficulty, (int minDays, int maxDays)>()
                {
                    { Difficulty.Easy, (minDays: 2, maxDays: 3) },
                    { Difficulty.Normal, (minDays: 3, maxDays: 4) },
                    { Difficulty.Hard, (minDays: 4, maxDays: 5) },
                }
            },
            {
                Disaster.Pest,
                new Dictionary<Difficulty, (int minDays, int maxDays)>()
                {
                    { Difficulty.Easy, (minDays: 1, maxDays: 1) },
                    { Difficulty.Normal, (minDays: 1, maxDays: 1) },
                    { Difficulty.Hard, (minDays: 1, maxDays: 1) }
                }
            }
        };

        private static readonly Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>> daysBetweenDisasters = new Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>>()
        {
            {
                Disaster.Drought,
                new Dictionary<Difficulty, (int minDays, int maxDays)>()
                {
                    { Difficulty.Easy, (minDays: 3, maxDays: 5) },
                    { Difficulty.Normal, (minDays: 2, maxDays: 4) },
                    { Difficulty.Hard, (minDays: 1, maxDays: 3) },
                }
            },
            {
                Disaster.Pest,
                new Dictionary<Difficulty, (int minDays, int maxDays)>()
                {
                    { Difficulty.Easy, (minDays: 2, maxDays: 3) },
                    { Difficulty.Normal, (minDays: 1, maxDays: 2) },
                    { Difficulty.Hard, (minDays: 1, maxDays: 1) }
                }
            }
        };

        private static readonly Dictionary<Difficulty, float> difficultyModifiers = new Dictionary<Difficulty, float>()
        {
            { Difficulty.Easy, 0.75f },
            { Difficulty.Normal, 1f },
            { Difficulty.Hard, 1.5f }
        };

        private static readonly Dictionary<Difficulty, float> secondaryDisasterChance = new Dictionary<Difficulty, float>()
        {
            { Difficulty.Easy, 0.1f }, // 10% chance that if a second disaster can happen it will happen
            { Difficulty.Normal, 0.5f }, // 50/50 chance that if a second disaster can happen it will happen
            { Difficulty.Hard, 1f } // Guaranteed that if a second disaster can happen it will happen
        };

        private static readonly float DEVIATION = 0.1f; // 10%
        private static readonly float TOLERANCE = 0.001f; // Amount a percentage can stray from 0 or 1 to be considered 0 or 1

        Dictionary<Disaster, int> activeDisasters = new Dictionary<Disaster, int>();
        Dictionary<Disaster, int> disastersOnCooldown = new Dictionary<Disaster, int>();

        public void DoDisaster(Disaster disaster, Difficulty currentDifficulty = Difficulty.Normal)
        {
            if (disaster == Disaster.None) return;
            if (!activeDisasters.ContainsKey(disaster) && !disastersOnCooldown.ContainsKey(disaster))
            {
                if (activeDisasters.Count > 0)
                {
                    if (secondaryDisasterChance[currentDifficulty] <= TOLERANCE) return; // ~0% chance that a secondary disaster can happen so don't do it
                    else if (secondaryDisasterChance[currentDifficulty] < 1f - TOLERANCE && Random.value > secondaryDisasterChance[currentDifficulty]) return;
                }
                int minDays = Mathf.Max(disasterDurations[disaster][currentDifficulty].minDays, 1); // Ensure min >= 1
                int maxDays = Mathf.Max(disasterDurations[disaster][currentDifficulty].maxDays, 1); // Ensure max >= 1
                int duration = Random.Range(minDays, maxDays + 1);
                activeDisasters.Add(disaster, duration);
                BroadcastDisasterStarted(disaster);
            }
        }

        public void DoDisaster(Season currentSeason, Difficulty currentDifficulty = Difficulty.Normal)
        {
            List<Disaster> disastersToDo = new List<Disaster>();
            foreach (Disaster disaster in System.Enum.GetValues(typeof(Disaster)))
            {
                if (disaster == Disaster.None) continue;
                float minChance = Mathf.Clamp01(baseDisasterChances[currentSeason][disaster].minChance); // Ensures that if the value is ever modified it remains between 0 and 1
                float maxChance = Mathf.Clamp01(baseDisasterChances[currentSeason][disaster].maxChance); // Ensures that if the value is ever modified it remains between 0 and 1
                float chance = Random.Range(minChance, maxChance); // Base rain chance between 0 and 1 no multipliers

                float deviation = Mathf.Max(Random.Range(1f - DEVIATION, 1f + DEVIATION), TOLERANCE); // Ensure the value > 0 for multiplication
                float difficultyModifier = Mathf.Max(difficultyModifiers[currentDifficulty], TOLERANCE); // Ensure the value > 0 for multiplication

                float totalChance = Mathf.Clamp01(chance * deviation * difficultyModifier); // Total chance with multipliers between 0 and 1

                if (totalChance <= TOLERANCE) continue; // ~0% chance don't do disaster
                else if (totalChance >= 1f - TOLERANCE) disastersToDo.Add(disaster); // ~100% do disaster
                else if (Random.value <= totalChance) disastersToDo.Add(disaster); // Succeeded roll do disaster
            }
            foreach (Disaster disaster in disasterPriorities[currentSeason]) // Do disasters based on seasonal priority
            {
                if (disastersToDo.Contains(disaster)) DoDisaster(disaster, currentDifficulty);
            }
        }

        public void TickDisasters(Difficulty currentDifficulty = Difficulty.Normal)
        {
            foreach (Disaster disaster in disastersOnCooldown.Keys.ToList())
            {
                --disastersOnCooldown[disaster];
                if (disastersOnCooldown[disaster] <= 0) disastersOnCooldown.Remove(disaster);
            }

            foreach (Disaster disaster in activeDisasters.Keys.ToList())
            {
                --activeDisasters[disaster];
                if (activeDisasters[disaster] <= 0)
                {
                    BroadcastDisasterEnded(disaster);
                    int minDays = Mathf.Max(daysBetweenDisasters[disaster][currentDifficulty].minDays, 1); // Ensure min >= 1
                    int maxDays = Mathf.Max(daysBetweenDisasters[disaster][currentDifficulty].maxDays, 1); // Ensure max >= 1
                    int cooldown = Random.Range(minDays, maxDays + 1);
                    disastersOnCooldown.Add(disaster, cooldown);
                    activeDisasters.Remove(disaster);
                }
            }
        }

        void BroadcastDisasterStarted(Disaster disaster)
        {
            GameEventType eventType = disaster switch
            {
                Disaster.Drought => GameEventType.DroughtDisasterEventStart,
                Disaster.Pest => GameEventType.PestDisasterEventStart,
                _ => GameEventType.None,
            };
            EventBus.Instance?.BroadcastEvent(eventType);
        }

        void BroadcastDisasterEnded(Disaster disaster)
        {
            GameEventType eventType = disaster switch
            {
                Disaster.Drought => GameEventType.DroughtDisasterEventEnd,
                Disaster.Pest => GameEventType.PestDisasterEventEnd,
                _ => GameEventType.None,
            };
            EventBus.Instance?.BroadcastEvent(eventType);
        }
    }
}
