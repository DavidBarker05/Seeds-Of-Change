using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FarmTile : ItemContainer, IEventListener
{
    [Header("Spawning")]
    [SerializeField]
    Transform spawnPosition;
    [Header("Start")]
    [SerializeField]
    SeedScribtableObject startingSeed;
    [SerializeField, Min(1)]
    int startingGrowthStage = 1;
    [Header("Crop")]
    [SerializeField, Min(1)]
    int waterNeededPerDay = 1;
    [SerializeField, Min(1)]
    int waterNeededPerDayInDrought = 2;
    [Header("Materials")]
    [SerializeField]
    Material soilMaterial;
    [SerializeField]
    Material wetSoilMaterial;
    [SerializeField]
    Material farmlandMaterial;
    [SerializeField]
    Material wetFarmlandMaterial;

    CropScriptableObject currentCrop;
    Renderer _renderer;

    GameObject currentCropPrefab;
    int currentGrowCycle;
    int daysWithoutWater;
    bool growthIsPaused;
    float currentYield;
    bool isDead;
    bool isInDrought;
    int timesWateredToday = 0;

    bool _isTilled;
    bool _isWet;

    bool IsTilled
    {
        get => _isTilled;
        set
        {
            _isTilled = value;
            UpdateMaterial();
        }
    }

    bool IsWet
    {
        get => _isWet;
        set
        {
            _isWet = value;
            UpdateMaterial();
        }
    }

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        EventBus.Instance?.AddEventListener(GameEventType.NewDayEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.ClearSkyWeatherEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.RainWeatherEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.DroughtDisasterEventStart, this);
        EventBus.Instance?.AddEventListener(GameEventType.DroughtDisasterEventEnd, this);
        EventBus.Instance?.AddEventListener(GameEventType.PlantWateredEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.CropHarvestedEvent, this);
    }

    void Start()
    {
        if (startingSeed == null)
        {
            IsTilled = false;
            return;
        }
        IsTilled = true;
        currentCrop = startingSeed.CropToPlant;
        currentGrowCycle = Mathf.Clamp(startingGrowthStage, 1, currentCrop.GrowthStages.Length);
        currentCropPrefab = Instantiate(currentCrop.GrowthStages[currentGrowCycle - 1].aliveCrop.cropPrefab, spawnPosition.position, spawnPosition.rotation);
        currentYield = 100f;
        daysWithoutWater = 0; // 0 because then sunny day gets called and sets it to 1
        timesWateredToday = 0;
        acceptedItems.Add(startingSeed, 1);
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.NewDayEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.ClearSkyWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.RainWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventStart, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventEnd, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.PlantWateredEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.CropHarvestedEvent, this);
    }

    protected override bool ExtraAddLogic(ItemScriptableObject item)
    {
        if (item is SeedScribtableObject seed)
        {
            if (currentCapacity > 0 || !IsTilled) return false;
            currentCrop = seed.CropToPlant;
            currentGrowCycle = 0;
            daysWithoutWater = 1;
            growthIsPaused = false;
            currentYield = 100f;
            isDead = false;
            return true;
        }
        return false;
    }

    protected override void ExtraClearLogic()
    {
        currentCrop = null;
        IsTilled = false;
        Destroy(currentCropPrefab);
    }

    public void WaterPlant()
    {
        ++timesWateredToday;
        if (timesWateredToday < (isInDrought && (currentCrop?.IsSusceptibleToDrought ?? true) ? waterNeededPerDayInDrought : waterNeededPerDay)) return;
        daysWithoutWater = 0;
        growthIsPaused = false;
        IsWet = true;
    }

    public void TillSoil() => IsTilled = true;

    void UpdateMaterial() => _renderer.material = IsTilled ? (IsWet ? wetFarmlandMaterial : farmlandMaterial) : (IsWet ? wetSoilMaterial : soilMaterial);

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.NewDayEvent:
                GoToNextStage();
                break;
            case GameEventType.ClearSkyWeatherEvent:
                ++daysWithoutWater;
                IsWet = false;
                break;
            case GameEventType.RainWeatherEvent:
                daysWithoutWater = 0;
                IsWet = true;
                break;
            case GameEventType.DroughtDisasterEventStart:
                isInDrought = true;
                break;
            case GameEventType.DroughtDisasterEventEnd:
                isInDrought = false;
                break;
            case GameEventType.PlantWateredEvent:
                if (parameters[0] is GameObject crop && crop == currentCropPrefab) WaterPlant();
                break;
            case GameEventType.CropHarvestedEvent:
                if (parameters[0] is GameObject harvested && harvested == currentCropPrefab)
                {
                    EventBus.Instance?.BroadcastEvent(GameEventType.GainCropEvent, currentCrop, isDead, currentGrowCycle, currentYield);
                    currentCrop = null;
                    Destroy(currentCropPrefab);
                }
                break;
            default:
                break;
        }
    }

    void GoToNextStage()
    {
        if (currentCrop == null || isDead) return;
        timesWateredToday = 0;
        CheckForWaterSetbacks();
        if (currentCrop == null) return;
        if (!isDead && !growthIsPaused) currentGrowCycle = Mathf.Clamp(++currentGrowCycle, 1, currentCrop.GrowthStages.Length);
        else if (currentGrowCycle == 0) ++currentGrowCycle;
        if (currentCropPrefab != null) Destroy(currentCropPrefab);
        GameObject cropPrefab = isDead ? currentCrop.GrowthStages[currentGrowCycle - 1].deadCrop.cropPrefab : currentCrop.GrowthStages[currentGrowCycle - 1].aliveCrop.cropPrefab;
        currentCropPrefab = Instantiate(cropPrefab, spawnPosition.position, spawnPosition.rotation);
    }

    void CheckForWaterSetbacks()
    {
        if (daysWithoutWater == 0) growthIsPaused = false;
        foreach (LackOfWaterSetback lackOfWaterSetback in currentCrop.LackOfWaterSetbacks)
        {
            if (daysWithoutWater < lackOfWaterSetback.daysWithoutWater) continue;
            float roll = Random.Range(0f, 100f);
            if (roll > lackOfWaterSetback.chance) continue;
            if (lackOfWaterSetback.type == LackOfWaterSetbackType.GrowthPause) growthIsPaused = true;
            else if (lackOfWaterSetback.type == LackOfWaterSetbackType.YieldReduction) currentYield = Mathf.Min(currentYield, 100f - lackOfWaterSetback.strength);
            else
            {
                isDead = true;
                return;
            }
        }
    }
}
