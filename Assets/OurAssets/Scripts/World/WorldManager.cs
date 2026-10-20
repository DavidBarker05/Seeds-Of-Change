using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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

    [SerializeField, FormerlySerializedAs("dateText")]
    TMP_Text m_DateText;
    [SerializeField, FormerlySerializedAs("foodText")]
    TMP_Text m_FoodText;
    [SerializeField, FormerlySerializedAs("disastersText")]
    TMP_Text m_DisastersText;
    [SerializeField, FormerlySerializedAs("rainVolume")]
    GameObject m_RainVolume;
    [SerializeField, FormerlySerializedAs("clearSkySkybox")]
    Material m_ClearSkySkybox;
    [SerializeField, FormerlySerializedAs("rainyDaySkybox")]
    Material m_RainyDaySkybox;
    [SerializeField, Min(1), FormerlySerializedAs("daysPerSeason")]
    int m_DaysPerSeason = 10;
    [SerializeField, FormerlySerializedAs("startingSeason")]
    Season m_StartingSeason = Season.RainySeason;
    [SerializeField, FormerlySerializedAs("startingWeather")]
    Weather m_StartingWeather = Weather.ClearSky;
    [SerializeField, FormerlySerializedAs("startingDifficulty")]
    Difficulty m_StartingDifficulty = Difficulty.Easy;
    [SerializeField, FormerlySerializedAs("itemFoodValues")]
    List<ItemFoodValue> m_ItemFoodValues;
    [SerializeField, Min(0), FormerlySerializedAs("familyStartingFood")]
    int m_FamilyStartingFood = 30;
    [SerializeField, Min(0), FormerlySerializedAs("communityStartingFood")]
    int m_CommunityStartingFood = 90;

    SeasonManager m_SeasonManager;
    WeatherManager m_WeatherManager;
    DisasterManager m_DisasterManager;
    FoodManager m_FoodManager;

    public int CurrentDay { get; private set; }
    public int CurrentDayInSeason { get; private set; }
    public int CurrentYear { get; private set; }
    public Season CurrentSeason => m_SeasonManager.CurrentSeason;
    public Weather CurrentWeather => m_WeatherManager.CurrentWeather;
    public Difficulty CurrentDifficulty { get; private set; }

    bool m_bCanResetForcedWeather;
    Weather m_ForcedWeather = Weather.None;
    bool m_bCanResetForcedDisaster;
    Disaster m_bForcedDisaster = Disaster.None;
    int m_DaysNeededToWin;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            m_SeasonManager = new SeasonManager(m_StartingSeason);
            m_WeatherManager = new WeatherManager();
            m_DisasterManager = new DisasterManager();
            m_FoodManager = new FoodManager(m_FamilyStartingFood, m_CommunityStartingFood, m_ItemFoodValues);
            CurrentDifficulty = m_StartingDifficulty;
            Season[] seasons = System.Enum.GetValues(typeof(Season)) as Season[];
            m_DaysNeededToWin = m_DaysPerSeason * seasons.Length / 2;
            EventBus.Instance?.AddEventListener("DroughtDisasterEventStart", this);
            EventBus.Instance?.AddEventListener("DroughtDisasterEventEnd", this);
            EventBus.Instance?.AddEventListener("ClearSkyWeatherEvent", this);
            EventBus.Instance?.AddEventListener("RainWeatherEvent", this);
            EventBus.Instance?.AddEventListener("SuccessfulSaleEvent", this);
            EventBus.Instance?.AddEventListener("SuccessfulStoreEvent", this);
        }
    }

    void Start()
    {
        if (Instance != this) return;
        m_bCanResetForcedWeather = true;
        m_bCanResetForcedDisaster = true;
        CurrentDay = 1;
        CurrentDayInSeason = 1;
        CurrentYear = 1;
        m_ForcedWeather = m_StartingWeather;
        DoWeather();
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener("DroughtDisasterEventStart", this);
        EventBus.Instance?.RemoveEventListener("DroughtDisasterEventEnd", this);
        EventBus.Instance?.RemoveEventListener("ClearSkyWeatherEvent", this);
        EventBus.Instance?.RemoveEventListener("RainWeatherEvent", this);
        EventBus.Instance?.RemoveEventListener("SuccessfulSaleEvent", this);
        EventBus.Instance?.RemoveEventListener("SuccessfulStoreEvent", this);
    }

    public void MoveToNextDay()
    {
        if (CurrentDay == m_DaysNeededToWin) EventBus.Instance?.BroadcastEvent("GameWinEvent"); // Only do once
        ++CurrentDay;
        ++CurrentDayInSeason;
        if (CurrentDayInSeason > m_DaysPerSeason) DoSeasonChange();
        EventBus.Instance?.BroadcastEvent("NewDayEvent");
        DoDisaster();
        DoWeather();
        m_FoodManager.DoFoodUsed(CurrentSeason);
        m_DateText.text = $"CURRENT YEAR: {CurrentYear}\nCURRENT DAY: {CurrentDay}\nCURRENT SEASON: {CurrentSeason}";
        m_FoodText.text = $"Family has {m_FoodManager.FamilyFood} food\nCommunity has {m_FoodManager.CommunityFood} food";
        m_DisastersText.text = $"Drought = {(m_DisasterManager.IsDroughtActive ? "Active" : "Inactive")}\n{(m_DisasterManager.IsPestActive ? "Pests have spawned today" : "No pests spawned today")}";
    }

    void DoSeasonChange()
    {
        m_SeasonManager.DoSeasonChange(); // Change the season to next season
        CurrentDayInSeason = 1; // Now first day in new season
        if (CurrentSeason == m_StartingSeason)
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
        if (m_ForcedWeather == Weather.None) m_WeatherManager.DoWeather(CurrentSeason, CurrentDifficulty);
        else
        {
            m_WeatherManager.DoWeather(m_ForcedWeather, CurrentDifficulty);
            if (m_bCanResetForcedWeather) m_ForcedWeather = Weather.None;
        }
    }

    void DoDisaster()
    {
        m_DisasterManager.TickDisasters(CurrentDifficulty);
        if (m_bForcedDisaster == Disaster.None) m_DisasterManager.DoDisaster(CurrentSeason, CurrentDifficulty);
        else
        {
            m_DisasterManager.DoDisaster(m_bForcedDisaster, CurrentDifficulty);
            if (m_bCanResetForcedDisaster) m_bForcedDisaster = Disaster.None;
        }
    }

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        switch (eventName)
        {
            case "DroughtDisasterEventStart":
                m_bCanResetForcedWeather = false;
                m_ForcedWeather = Weather.ClearSky;
                break;
            case "DroughtDisasterEventEnd":
                m_bCanResetForcedWeather = true;
                m_ForcedWeather = Weather.Rain;
                break;
            case "ClearSkyWeatherEvent":
                if (m_RainVolume != null) m_RainVolume.SetActive(false);
                if (m_ClearSkySkybox != null && RenderSettings.skybox != m_ClearSkySkybox) RenderSettings.skybox = m_ClearSkySkybox;
                break;
            case "RainWeatherEvent":
                if (m_RainVolume != null) m_RainVolume.SetActive(true);
                if (m_RainyDaySkybox != null && RenderSettings.skybox != m_RainyDaySkybox) RenderSettings.skybox = m_RainyDaySkybox;
                break;
            case "SuccessfulSaleEvent":
                if (parameters[0] is ItemScriptableObject soldItem && parameters[1] is int soldAmount) m_FoodManager.AddCommunityFood(soldItem, soldAmount);
                break;
            case "SuccessfulStoreEvent":
                if (parameters[0] is ItemScriptableObject storedItem && parameters[1] is int storedAmount) m_FoodManager.AddFamilyFood(storedItem, storedAmount);
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
        private static readonly Dictionary<Season, (float minChance, float maxChance)> s_BaseRainChances = new Dictionary<Season, (float minChance, float maxChance)>()
        {
            { Season.RainySeason, (minChance: 0.45f, maxChance: 0.65f) },
            { Season.CoolSeason, (minChance: 0.25f, maxChance: 0.4f) },
            { Season.HotSeason, (minChance: 0.08f, maxChance: 0.15f) },
            { Season.DrySeason, (minChance: 0.03f, maxChance: 0.08f) }
        };

        private static readonly Dictionary<Difficulty, float> s_DifficultyModifiers = new Dictionary<Difficulty, float>()
        {
            { Difficulty.Easy, 1f },
            { Difficulty.Normal, 0.9f },
            { Difficulty.Hard, 0.7f }
        };

        private static readonly Dictionary<Difficulty, (float minAccumulation, float maxAccumulation, float accumulationDecrease, float accumulationIncrease)> s_DifficultyAccumulations = new Dictionary<Difficulty, (float minAccumulation, float maxAccumulation, float accumulationDecrease, float accumulationIncrease)>()
        {
            { Difficulty.Easy, (minAccumulation: -0.05f, maxAccumulation: 0.15f, accumulationDecrease: -0.005f, accumulationIncrease: 0.015f) },
            { Difficulty.Normal, (minAccumulation: -0.1f, maxAccumulation: 0.1f, accumulationDecrease: -0.01f, accumulationIncrease: 0.01f) },
            { Difficulty.Hard, (minAccumulation: -0.15f, maxAccumulation: 0.05f, accumulationDecrease: -0.015f, accumulationIncrease: 0.005f) }
        };

        private static readonly float s_Deviation = 0.1f; // 10%
        private static readonly float s_Tolerance = 0.001f; // Amount a percentage can stray from 0 or 1 to be considered 0 or 1

        private float m_CurrentAccumulation = 0f;

        public Weather CurrentWeather { get; private set; }

        public WeatherManager() => CurrentWeather = Weather.ClearSky;

        public void DoWeather(Weather weather, Difficulty currentDifficulty = Difficulty.Normal)
        {
            if (weather == Weather.ClearSky)
            {
                if (m_CurrentAccumulation < 0f) m_CurrentAccumulation = 0f; // Reset accumulation
                else
                {
                    float maxAccumulation = Mathf.Max(s_DifficultyAccumulations[currentDifficulty].maxAccumulation, 0f); // Ensure max >= 0
                    float accumulationIncrease = Mathf.Max(s_DifficultyAccumulations[currentDifficulty].accumulationIncrease, 0f); // Ensure increase >= 0
                    m_CurrentAccumulation = Mathf.Clamp(m_CurrentAccumulation + accumulationIncrease, 0f, maxAccumulation);
                }
            }
            else if (weather == Weather.Rain)
            {
                if (m_CurrentAccumulation > 0f) m_CurrentAccumulation = 0f; // Reset accumulation
                else
                {
                    float minAccumulation = Mathf.Min(s_DifficultyAccumulations[currentDifficulty].minAccumulation, 0f); // Ensure min <= 0
                    float accumulationDecrease = Mathf.Min(s_DifficultyAccumulations[currentDifficulty].accumulationDecrease, 0f); // Ensure decrease <= 0
                    m_CurrentAccumulation = Mathf.Clamp(m_CurrentAccumulation + accumulationDecrease, minAccumulation, 0f);
                }
            }
            CurrentWeather = weather;
            string eventName = CurrentWeather switch
            {
                Weather.ClearSky => "ClearSkyWeatherEvent",
                Weather.Rain => "RainWeatherEvent",
                _ => string.Empty
            };
            EventBus.Instance?.BroadcastEvent(eventName);
        }

        public void DoWeather(Season currentSeason, Difficulty currentDifficulty = Difficulty.Normal)
        {
            float minRainChance = Mathf.Clamp01(s_BaseRainChances[currentSeason].minChance); // Ensures that if the value is ever modified it remains between 0 and 1
            float maxRainChance = Mathf.Clamp01(s_BaseRainChances[currentSeason].maxChance); // Ensures that if the value is ever modified it remains between 0 and 1
            float baseRainChance = Random.Range(minRainChance, maxRainChance); // Base rain chance between 0 and 1 no multipliers or accumulation
            float deviation = Mathf.Max(Random.Range(1f - s_Deviation, 1f + s_Deviation), s_Tolerance); // Ensure the value > 0 for multiplication
            float difficultyModifier = Mathf.Max(s_DifficultyModifiers[currentDifficulty], s_Tolerance); // Ensure the value > 0 for multiplication
            float totalRainChance = Mathf.Clamp01(baseRainChance * deviation * difficultyModifier + m_CurrentAccumulation); // Total chance with multipliers and accumulation between 0 and 1
            if (totalRainChance <= s_Tolerance) DoWeather(Weather.ClearSky, currentDifficulty); // ~0% chance do clear sky
            else if (totalRainChance >= 1f - s_Tolerance) DoWeather(Weather.Rain, currentDifficulty); // ~100% do rain
            else DoWeather(Random.value > totalRainChance ? Weather.ClearSky : Weather.Rain, currentDifficulty); // Random weather
        }
    }

    private class DisasterManager
    {
        private static readonly Dictionary<Season, Dictionary<Disaster, (float minChance, float maxChance)>> s_BaseDisasterChances = new Dictionary<Season, Dictionary<Disaster, (float minChance, float maxChance)>>()
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

        private static readonly Dictionary<Season, List<Disaster>> s_DisasterPriorities = new Dictionary<Season, List<Disaster>>()
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

        private static readonly Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>> s_DisasterDurations = new Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>>()
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

        private static readonly Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>> s_DaysBetweenDisasters = new Dictionary<Disaster, Dictionary<Difficulty, (int minDays, int maxDays)>>()
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

        private static readonly Dictionary<Difficulty, float> s_DifficultyModifiers = new Dictionary<Difficulty, float>()
        {
            { Difficulty.Easy, 0.75f },
            { Difficulty.Normal, 1f },
            { Difficulty.Hard, 1.5f }
        };

        private static readonly Dictionary<Difficulty, float> s_SecondaryDisasterChance = new Dictionary<Difficulty, float>()
        {
            { Difficulty.Easy, 0.1f }, // 10% chance that if a second disaster can happen it will happen
            { Difficulty.Normal, 0.5f }, // 50/50 chance that if a second disaster can happen it will happen
            { Difficulty.Hard, 1f } // Guaranteed that if a second disaster can happen it will happen
        };

        private static readonly float s_Deviation = 0.1f; // 10%
        private static readonly float s_Tolerance = 0.001f; // Amount a percentage can stray from 0 or 1 to be considered 0 or 1

        readonly Dictionary<Disaster, int> m_ActiveDisasters = new Dictionary<Disaster, int>();
        readonly Dictionary<Disaster, int> m_DisastersOnCooldown = new Dictionary<Disaster, int>();

        public bool IsDroughtActive => m_ActiveDisasters.ContainsKey(Disaster.Drought);
        public bool IsPestActive => m_ActiveDisasters.ContainsKey(Disaster.Pest);

        public void DoDisaster(Disaster disaster, Difficulty currentDifficulty = Difficulty.Normal)
        {
            if (disaster == Disaster.None) return;
            if (!m_ActiveDisasters.ContainsKey(disaster) && !m_DisastersOnCooldown.ContainsKey(disaster))
            {
                if (m_ActiveDisasters.Count > 0)
                {
                    if (s_SecondaryDisasterChance[currentDifficulty] <= s_Tolerance) return; // ~0% chance that a secondary disaster can happen so don't do it
                    else if (s_SecondaryDisasterChance[currentDifficulty] < 1f - s_Tolerance && Random.value > s_SecondaryDisasterChance[currentDifficulty]) return;
                }
                int minDays = Mathf.Max(s_DisasterDurations[disaster][currentDifficulty].minDays, 1); // Ensure min >= 1
                int maxDays = Mathf.Max(s_DisasterDurations[disaster][currentDifficulty].maxDays, 1); // Ensure max >= 1
                int duration = Random.Range(minDays, maxDays + 1);
                m_ActiveDisasters.Add(disaster, duration);
                BroadcastDisasterStarted(disaster);
            }
        }

        public void DoDisaster(Season currentSeason, Difficulty currentDifficulty = Difficulty.Normal)
        {
            List<Disaster> disastersToDo = new List<Disaster>();
            foreach (Disaster disaster in System.Enum.GetValues(typeof(Disaster)))
            {
                if (disaster == Disaster.None) continue;
                float minChance = Mathf.Clamp01(s_BaseDisasterChances[currentSeason][disaster].minChance); // Ensures that if the value is ever modified it remains between 0 and 1
                float maxChance = Mathf.Clamp01(s_BaseDisasterChances[currentSeason][disaster].maxChance); // Ensures that if the value is ever modified it remains between 0 and 1
                float chance = Random.Range(minChance, maxChance); // Base rain chance between 0 and 1 no multipliers
                float deviation = Mathf.Max(Random.Range(1f - s_Deviation, 1f + s_Deviation), s_Tolerance); // Ensure the value > 0 for multiplication
                float difficultyModifier = Mathf.Max(s_DifficultyModifiers[currentDifficulty], s_Tolerance); // Ensure the value > 0 for multiplication
                float totalChance = Mathf.Clamp01(chance * deviation * difficultyModifier); // Total chance with multipliers between 0 and 1
                if (totalChance <= s_Tolerance) continue; // ~0% chance don't do disaster
                else if (totalChance >= 1f - s_Tolerance) disastersToDo.Add(disaster); // ~100% do disaster
                else if (Random.value <= totalChance) disastersToDo.Add(disaster); // Succeeded roll do disaster
            }
            foreach (Disaster disaster in s_DisasterPriorities[currentSeason]) // Do disasters based on seasonal priority
            {
                if (disastersToDo.Contains(disaster)) DoDisaster(disaster, currentDifficulty);
            }
        }

        public void TickDisasters(Difficulty currentDifficulty = Difficulty.Normal)
        {
            foreach (Disaster disaster in m_DisastersOnCooldown.Keys.ToList())
            {
                --m_DisastersOnCooldown[disaster];
                if (m_DisastersOnCooldown[disaster] <= 0) m_DisastersOnCooldown.Remove(disaster);
            }
            foreach (Disaster disaster in m_ActiveDisasters.Keys.ToList())
            {
                --m_ActiveDisasters[disaster];
                if (m_ActiveDisasters[disaster] <= 0)
                {
                    BroadcastDisasterEnded(disaster);
                    int minDays = Mathf.Max(s_DaysBetweenDisasters[disaster][currentDifficulty].minDays, 1); // Ensure min >= 1
                    int maxDays = Mathf.Max(s_DaysBetweenDisasters[disaster][currentDifficulty].maxDays, 1); // Ensure max >= 1
                    int cooldown = Random.Range(minDays, maxDays + 1);
                    m_DisastersOnCooldown.Add(disaster, cooldown);
                    m_ActiveDisasters.Remove(disaster);
                }
            }
        }

        void BroadcastDisasterStarted(Disaster disaster)
        {
            string eventName = disaster switch
            {
                Disaster.Drought => "DroughtDisasterEventStart",
                Disaster.Pest => "PestDisasterEventStart",
                _ => string.Empty,
            };
            EventBus.Instance?.BroadcastEvent(eventName);
        }

        void BroadcastDisasterEnded(Disaster disaster)
        {
            string eventName = disaster switch
            {
                Disaster.Drought => "DroughtDisasterEventEnd",
                Disaster.Pest => "PestDisasterEventEnd",
                _ => string.Empty,
            };
            EventBus.Instance?.BroadcastEvent(eventName);
        }
    }

    // For the food manager we want it to be more consistent in how much food is used up
    // We don't randomise it every day like the others but it does get harder for every difficulty
    // Unlike the others this does scale forever though
    private class FoodManager
    {
        private static readonly Dictionary<Season, (int familyNeeds, int communityNeeds)> s_DailyFoodNeededPerSeason = new Dictionary<Season, (int familyNeeds, int communityNeeds)>()
        {
            { Season.RainySeason, (familyNeeds: 4, communityNeeds: 12) },
            { Season.CoolSeason, (familyNeeds: 3, communityNeeds: 9) },
            { Season.HotSeason, (familyNeeds: 5, communityNeeds: 15) },
            { Season.DrySeason, (familyNeeds: 2, communityNeeds: 6) }
        };

        private static readonly Dictionary<Difficulty, float> s_DifficultyModifiers = new Dictionary<Difficulty, float>()
        {
            { Difficulty.Easy, 1f },
            { Difficulty.Normal, 1.1f },
            { Difficulty.Hard, 1.2f }
        };

        private static readonly Dictionary<Difficulty, int> s_InterestStartingYears = new Dictionary<Difficulty, int>()
        {
            { Difficulty.Easy, 4 }, // Started on easy means years 4+ apply interest
            { Difficulty.Normal, 3 }, // Started on normal means years 3+ apply interest
            { Difficulty.Hard, 2 } // Started on hard means years 2+ apply interest
        };

        private static readonly float s_CompoundInterest = 1.05f; // 5% more food every year after 3rd year

        public int FamilyFood { get; private set; }
        public int CommunityFood { get; private set; }

        readonly Dictionary<ItemScriptableObject, int> m_ItemFoodValues = new Dictionary<ItemScriptableObject, int>();
        private int m_InterestStartingYear;

        public FoodManager(int startingFamilyFood, int startingCommunityFood, List<ItemFoodValue> itemFoodValueList, Difficulty startingDifficulty = Difficulty.Easy)
        {
            FamilyFood = startingFamilyFood;
            CommunityFood = startingCommunityFood;
            foreach (ItemFoodValue itemFoodValue in itemFoodValueList)
            {
                if (!m_ItemFoodValues.ContainsKey(itemFoodValue.Item)) m_ItemFoodValues.Add(itemFoodValue.Item, itemFoodValue.FoodValue);
            }
            m_InterestStartingYear = s_InterestStartingYears[startingDifficulty];
        }

        public void DoFoodUsed(Season currentSeason, Difficulty currentDifficulty = Difficulty.Normal, int currentYear = 1)
        {
            float difficultyModifier = s_DifficultyModifiers[currentDifficulty];
            float interest = currentYear < m_InterestStartingYear ? 1f : s_CompoundInterest; // First three years it doesn't scale after that it scales exponentially
            int familyNeeds = (int)(s_DailyFoodNeededPerSeason[currentSeason].familyNeeds * difficultyModifier * interest); // Truncation is fine
            int communityNeeds = (int)(s_DailyFoodNeededPerSeason[currentSeason].communityNeeds * difficultyModifier * interest); // Truncation is fine
            FamilyFood -= familyNeeds;
            CommunityFood -= communityNeeds;
            if (FamilyFood <= 0 || CommunityFood <= 0) EventBus.Instance?.BroadcastEvent("GameLoseEvent", FamilyFood, CommunityFood);
        }

        public void AddFamilyFood(ItemScriptableObject food, int amount)
        {
            if (m_ItemFoodValues.ContainsKey(food)) FamilyFood += m_ItemFoodValues[food] * amount;
        }

        public void AddCommunityFood(ItemScriptableObject food, int amount)
        {
            if (m_ItemFoodValues.ContainsKey(food)) CommunityFood += m_ItemFoodValues[food] * amount;
        }
    }
}

[System.Serializable]
public struct ItemFoodValue
{
    [FormerlySerializedAs("item")]
    public ItemScriptableObject Item;
    [FormerlySerializedAs("foodValue")]
    public int FoodValue;
}
