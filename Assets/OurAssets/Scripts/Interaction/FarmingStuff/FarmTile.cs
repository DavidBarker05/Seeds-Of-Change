using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Renderer))]
public class FarmTile : ItemContainer, IEventListener
{
    [SerializeField, FormerlySerializedAs("pestPrefab")]
    GameObject m_PestPrefab;
    [Header("Spawning")]
    [SerializeField, FormerlySerializedAs("spawnPosition")]
    Transform m_SpawnPosition;
    [Header("Start")]
    [SerializeField, FormerlySerializedAs("startingSeed")]
    SeedScribtableObject m_StartingSeed;
    [SerializeField, Min(1), FormerlySerializedAs("startingGrowthStage")]
    int m_StartingGrowthStage = 1;
    [Header("Crop")]
    [SerializeField, Min(1), FormerlySerializedAs("waterNeededPerDay")]
    int m_WaterNeededPerDay = 1;
    [SerializeField, Min(1), FormerlySerializedAs("waterNeededPerDayInDrought")]
    int m_WaterNeededPerDayInDrought = 2;
    [Header("Materials")]
    [SerializeField, FormerlySerializedAs("soilMaterial")]
    Material m_SoilMaterial;
    [SerializeField, FormerlySerializedAs("farmlandMaterial")]
    Material m_FarmlandMaterial;

    private static readonly Dictionary<Difficulty, float> s_DifficultyModifiers = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Easy, 0.8f },
        { Difficulty.Normal, 1f },
        { Difficulty.Hard, 1.2f }
    };

    private static readonly float s_Deviation = 0.1f; // 10%
    private static readonly float s_Tolerance = 0.001f; // Amount a percentage can stray from 0 or 1 to be considered 0 or 1

    CropScriptableObject m_CurrentCrop;
    Renderer m_Renderer;

    GameObject m_CurrentCropPrefab;
    int m_CurrentGrowCycle;
    int m_DaysWithoutWater;
    bool m_bGrowthPausedFromNoWater;
    float m_YieldReductionFromNoWater;
    bool m_bDead;
    bool m_bDrought;
    int m_TimesWateredToday = 0;
    GameObject m_CurrentPest;
    float m_PassivePestPreventionPercent = 0f;
    int m_PassivePestPreventionDuration = 0;
    int m_DaysWithPests;
    bool m_bGrowthPausedFromPest;
    float m_YieldReductionFromPest;

    float CurrentYield => 100f - m_YieldReductionFromNoWater - m_YieldReductionFromPest;

    bool m_bTilled;
    bool IsTilled
    {
        get => m_bTilled;
        set
        {
            m_bTilled = value;
            UpdateMaterial();
        }
    }

    bool m_bWet;
    bool IsWet
    {
        get => m_bWet;
        set
        {
            m_bWet = value;
            UpdateMaterial();
        }
    }

    void Awake()
    {
        m_Renderer = GetComponent<Renderer>();
        EventBus.Instance?.AddEventListener("NewDayEvent", this);
        EventBus.Instance?.AddEventListener("ClearSkyWeatherEvent", this);
        EventBus.Instance?.AddEventListener("RainWeatherEvent", this);
        EventBus.Instance?.AddEventListener("DroughtDisasterEventStart", this);
        EventBus.Instance?.AddEventListener("DroughtDisasterEventEnd", this);
        EventBus.Instance?.AddEventListener("PlantWateredEvent", this);
        EventBus.Instance?.AddEventListener("PestWateredEvent", this);
        EventBus.Instance?.AddEventListener("CropHarvestedEvent", this);
        EventBus.Instance?.AddEventListener("PestDisasterEventStart", this);
        EventBus.Instance?.AddEventListener("CropPassivePestAppliedEvent", this);
        EventBus.Instance?.AddEventListener("PestPassivePestAppliedEvent", this);
        EventBus.Instance?.AddEventListener("CropActivePestAppliedEvent", this);
        EventBus.Instance?.AddEventListener("PestActivePestAppliedEvent", this);
    }

    void Start()
    {
        if (m_StartingSeed == null)
        {
            IsTilled = false;
            return;
        }
        IsTilled = true;
        m_CurrentCrop = m_StartingSeed.CropToPlant;
        m_CurrentGrowCycle = Mathf.Clamp(m_StartingGrowthStage, 1, m_CurrentCrop.GrowthStages.Length);
        m_CurrentCropPrefab = Instantiate(m_CurrentCrop.GrowthStages[m_CurrentGrowCycle - 1].aliveCrop.cropPrefab, m_SpawnPosition.position, m_SpawnPosition.rotation);
        m_YieldReductionFromNoWater = 0f;
        m_YieldReductionFromPest = 0f;
        m_DaysWithoutWater = 0; // 0 because then sunny day gets called and sets it to 1
        m_TimesWateredToday = 0;
        m_DaysWithPests = 0;
        acceptedItems.Add(m_StartingSeed, 1);
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener("NewDayEvent", this);
        EventBus.Instance?.RemoveEventListener("ClearSkyWeatherEvent", this);
        EventBus.Instance?.RemoveEventListener("RainWeatherEvent", this);
        EventBus.Instance?.RemoveEventListener("DroughtDisasterEventStart", this);
        EventBus.Instance?.RemoveEventListener("DroughtDisasterEventEnd", this);
        EventBus.Instance?.RemoveEventListener("PlantWateredEvent", this);
        EventBus.Instance?.RemoveEventListener("PestWateredEvent", this);
        EventBus.Instance?.RemoveEventListener("CropHarvestedEvent", this);
        EventBus.Instance?.RemoveEventListener("PestDisasterEventStart", this);
        EventBus.Instance?.RemoveEventListener("CropPassivePestAppliedEvent", this);
        EventBus.Instance?.RemoveEventListener("PestPassivePestAppliedEvent", this);
        EventBus.Instance?.RemoveEventListener("CropActivePestAppliedEvent", this);
        EventBus.Instance?.RemoveEventListener("PestActivePestAppliedEvent", this);
    }

    protected override AddItemInfo ExtraAddLogic(ItemScriptableObject item)
    {
        if (item is SeedScribtableObject seed)
        {
            if (currentCapacity > 0 || !IsTilled) return new AddItemInfo() { WasAdded = false };
            m_CurrentCrop = seed.CropToPlant;
            m_CurrentGrowCycle = 0;
            m_DaysWithoutWater = 1;
            m_bGrowthPausedFromNoWater = false;
            m_YieldReductionFromNoWater = 0f;
            m_YieldReductionFromPest = 0f;
            m_bDead = false;
            m_DaysWithPests = 0;
            return new AddItemInfo() { WasAdded = true };
        }
        return new AddItemInfo() { WasAdded = false };
    }

    protected override void ExtraClearLogic()
    {
        m_CurrentCrop = null;
        IsTilled = false;
        Destroy(m_CurrentCropPrefab);
        m_CurrentCropPrefab = null;
        if (m_CurrentPest != null)
        {
            Destroy(m_CurrentPest);
            m_CurrentPest = null;
        }
    }

    public void WaterPlant()
    {
        ++m_TimesWateredToday;
        if (m_TimesWateredToday < (m_bDrought && (m_CurrentCrop?.IsSusceptibleToDrought ?? true) ? m_WaterNeededPerDayInDrought : m_WaterNeededPerDay)) return;
        m_DaysWithoutWater = 0;
        m_bGrowthPausedFromNoWater = false;
        IsWet = true;
    }

    public void TillSoil() => IsTilled = true;

    public void ApplyPassivePesticide(float percentReduction, int duration)
    {
        if (percentReduction <= 0 || duration <= 0) return;
        m_PassivePestPreventionPercent = percentReduction;
        m_PassivePestPreventionDuration = duration; // Reset duration to make
    }

    public void ApplyActivePesticide()
    {
        if (m_CurrentPest != null)
        {
            Destroy(m_CurrentPest);
            m_CurrentPest = null;
            m_DaysWithPests = 0;
            m_bGrowthPausedFromPest = false;
        }
    }

    void UpdateMaterial()
    {
        m_Renderer.material = IsTilled ? m_FarmlandMaterial : m_SoilMaterial;
        m_Renderer.material.SetInt("_IsWet", IsWet ? 1 : 0);
    }

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        switch (eventName)
        {
            case "NewDayEvent":
                if (m_PassivePestPreventionDuration > 0) --m_PassivePestPreventionDuration;
                if (m_PassivePestPreventionDuration <= 0) m_PassivePestPreventionPercent = 0f;
                GoToNextStage();
                break;
            case "ClearSkyWeatherEvent":
                ++m_DaysWithoutWater;
                IsWet = false;
                break;
            case "RainWeatherEvent":
                m_DaysWithoutWater = 0;
                IsWet = true;
                break;
            case "DroughtDisasterEventStart":
                m_bDrought = true;
                break;
            case "DroughtDisasterEventEnd":
                m_bDrought = false;
                break;
            case "PlantWateredEvent": // Can water farm tile, crop or pest
                if (parameters[0] is GameObject wateredCrop && wateredCrop == m_CurrentCropPrefab) WaterPlant();
                break;
            case "PestWateredEvent": // Can water farm tile, crop or pest
                if (parameters[0] is GameObject wateredPest && wateredPest == m_CurrentPest) WaterPlant();
                break;
            case "CropHarvestedEvent":
                if (parameters[0] is GameObject harvestedCrop && harvestedCrop == m_CurrentCropPrefab)
                {
                    EventBus.Instance.BroadcastEvent("GainCropEvent", m_CurrentCrop, m_bDead, m_CurrentGrowCycle, CurrentYield);
                    ClearItems();
                }
                break;
            case "PestDisasterEventStart":
                SpawnPest(WorldManager.Instance?.CurrentDifficulty ?? Difficulty.Easy);
                break;
            case "CropPassivePestAppliedEvent": // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject passivePestCrop && passivePestCrop == m_CurrentCropPrefab)
                {
                    if (parameters[1] is float percentReduction && parameters[2] is int duration) ApplyPassivePesticide(percentReduction, duration);
                }
                break;
            case "PestPassivePestAppliedEvent": // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject passivePestPest && passivePestPest == m_CurrentPest)
                {
                    if (parameters[1] is float percentReduction && parameters[2] is int duration) ApplyPassivePesticide(percentReduction, duration);
                }
                break;
            case "CropActivePestAppliedEvent": // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject activePestCrop && activePestCrop == m_CurrentCropPrefab) ApplyActivePesticide();
                break;
            case "PestActivePestAppliedEvent": // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject activePestPest && activePestPest == m_CurrentPest) ApplyActivePesticide();
                break;
            default:
                break;
        }
    }

    void GoToNextStage()
    {
        if (m_CurrentCrop == null || m_bDead) return;
        m_TimesWateredToday = 0;
        CheckForWaterSetbacks();
        CheckForPestSetbacks();
        if (m_CurrentCrop == null) return;
        if (!m_bDead && !m_bGrowthPausedFromNoWater && !m_bGrowthPausedFromPest) m_CurrentGrowCycle = Mathf.Clamp(++m_CurrentGrowCycle, 1, m_CurrentCrop.GrowthStages.Length);
        else if (m_CurrentGrowCycle == 0) ++m_CurrentGrowCycle;
        if (m_CurrentCropPrefab != null) Destroy(m_CurrentCropPrefab);
        GameObject cropPrefab = m_bDead ? m_CurrentCrop.GrowthStages[m_CurrentGrowCycle - 1].deadCrop.cropPrefab : m_CurrentCrop.GrowthStages[m_CurrentGrowCycle - 1].aliveCrop.cropPrefab;
        m_CurrentCropPrefab = Instantiate(cropPrefab, m_SpawnPosition.position, m_SpawnPosition.rotation);
    }

    void CheckForWaterSetbacks()
    {
        if (m_DaysWithoutWater == 0) m_bGrowthPausedFromNoWater = false;
        foreach (LackOfWaterSetback lackOfWaterSetback in m_CurrentCrop.LackOfWaterSetbacks)
        {
            if (m_DaysWithoutWater < lackOfWaterSetback.daysWithoutWater) continue;
            float chance = lackOfWaterSetback.chance / 100f;
            if (chance <= s_Tolerance) continue; // ~0%
            else if (chance < 1f - s_Tolerance && Random.value > chance) continue; // Less than ~100% and failed roll
            if (lackOfWaterSetback.type == LackOfWaterSetbackType.GrowthPause) m_bGrowthPausedFromNoWater = true;
            else if (lackOfWaterSetback.type == LackOfWaterSetbackType.YieldReduction) m_YieldReductionFromNoWater = Mathf.Max(m_YieldReductionFromNoWater, lackOfWaterSetback.strength);
            else
            {
                m_bDead = true;
                return;
            }
        }
    }

    void CheckForPestSetbacks()
    {
        if (m_CurrentPest == null)
        {
            m_DaysWithPests = 0;
            m_bGrowthPausedFromPest = false;
            return;
        }
        ++m_DaysWithPests;
        if (m_DaysWithPests >= 1) m_bGrowthPausedFromPest = true;
        if (m_DaysWithPests >= 2) m_YieldReductionFromPest = Mathf.Max(m_YieldReductionFromPest, 20f);
        if (m_DaysWithPests >= 3) m_bDead = true;
    }

    void SpawnPest(Difficulty currentDifficulty = Difficulty.Easy)
    {
        if (m_CurrentCrop == null) return;
        float susceptibility = Mathf.Clamp01(m_CurrentCrop.SusceptibilityToPests / 100f);
        float deviation = Mathf.Max(Random.Range(1f - s_Deviation, 1f + s_Deviation), s_Tolerance); // Ensure the value > 0 for multiplication
        float difficultyModifier = Mathf.Max(s_DifficultyModifiers[currentDifficulty], s_Tolerance); // Ensure the value > 0 for multiplication
        float passivePrevention = m_PassivePestPreventionDuration > 0 ? m_PassivePestPreventionPercent / 100f : 0f;
        float totalSusceptibility = Mathf.Clamp01(susceptibility * deviation * difficultyModifier - passivePrevention);
        if (totalSusceptibility <= s_Tolerance) return; // ~0%
        else if (totalSusceptibility < 1f - s_Tolerance && Random.value > totalSusceptibility) return; // Less than ~100% and failed roll
        if (m_CurrentPest == null) m_CurrentPest = Instantiate(m_PestPrefab, m_SpawnPosition.position, m_SpawnPosition.rotation);
    }
}
