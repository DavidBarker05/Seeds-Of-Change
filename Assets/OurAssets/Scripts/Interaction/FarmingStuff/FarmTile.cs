using UnityEngine;

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

    CropScriptableObject currentCrop;

    GameObject currentCropPrefab;
    int currentGrowCycle;
    int daysWithoutWater;
    bool growthIsPaused;
    float currentYield;
    bool isDead;
    bool isInDrought;
    int timesWateredToday = 0;

    void Start()
    {
        EventBus.Instance?.AddEventListener(GameEventType.NewDayEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.ClearSkyWeatherEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.RainWeatherEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.DroughtDisasterEventStart, this);
        EventBus.Instance?.AddEventListener(GameEventType.DroughtDisasterEventEnd, this);
        if (startingSeed == null) return;
        currentCrop = startingSeed.CropToPlant;
        currentGrowCycle = Mathf.Clamp(startingGrowthStage, 1, currentCrop.GrowthStages.Length);
        currentCropPrefab = Instantiate(currentCrop.GrowthStages[currentGrowCycle - 1].aliveCrop.cropPrefab, spawnPosition.position, spawnPosition.rotation);
        daysWithoutWater = 1;
        waterNeededPerDay = 1;
        acceptedItems.Add(startingSeed, 1);
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.NewDayEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.ClearSkyWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.RainWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventStart, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventEnd, this);
    }

    protected override bool ExtraAddLogic(ItemScriptableObject item)
    {
        if (item is SeedScribtableObject seed)
        {
            if (currentCapacity > 0) return true; // Handled because can't take more crops
            currentCrop = seed.CropToPlant;
            currentGrowCycle = 0;
            daysWithoutWater = 1;
            growthIsPaused = false;
            currentYield = 100f;
            isDead = false;
            waterNeededPerDay = 1;
        }
        else if (item.ItemName == "Watering Can")
        {
            ++timesWateredToday;
            if (timesWateredToday < (isInDrought ? waterNeededPerDayInDrought : waterNeededPerDay)) return true; // Handled the watering and don't want to add it to the stored items
            daysWithoutWater = 0;
            growthIsPaused = false;
            return true; // Handled the watering and don't want to add it to the stored items
        }
        return false;
    }

    protected override void ExtraClearLogic()
    {
        currentCrop = null;
        Destroy(currentCropPrefab);
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.NewDayEvent:
                GoToNextStage();
                break;
            case GameEventType.ClearSkyWeatherEvent:
                ++daysWithoutWater;
                break;
            case GameEventType.RainWeatherEvent:
                daysWithoutWater = 0;
                break;
            case GameEventType.DroughtDisasterEventStart:
                isInDrought = true;
                break;
            case GameEventType.DroughtDisasterEventEnd:
                isInDrought = false;
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
        if (currentCropPrefab != null) Destroy(currentCropPrefab);
        GameObject cropPrefab = isDead ? currentCrop.GrowthStages[currentGrowCycle - 1].deadCrop.cropPrefab : currentCrop.GrowthStages[currentGrowCycle - 1].aliveCrop.cropPrefab;
        currentCropPrefab = Instantiate(cropPrefab, spawnPosition.position, spawnPosition.rotation);
    }

    void CheckForWaterSetbacks()
    {
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
