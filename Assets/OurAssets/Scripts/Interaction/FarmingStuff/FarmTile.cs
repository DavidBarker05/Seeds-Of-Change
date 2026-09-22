using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Renderer))]
public class FarmTile : ItemContainer, IEventListener
{
    [SerializeField]
    GameObject pestPrefab;
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

    private static readonly Dictionary<Difficulty, float> difficultyModifiers = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Easy, 0.8f },
        { Difficulty.Normal, 1f },
        { Difficulty.Hard, 1.2f }
    };

    private static readonly float DEVIATION = 0.1f; // 10%
    private static readonly float TOLERANCE = 0.001f; // Amount a percentage can stray from 0 or 1 to be considered 0 or 1

    CropScriptableObject currentCrop;
    Renderer _renderer;

    GameObject currentCropPrefab;
    int currentGrowCycle;
    int daysWithoutWater;
    bool growthIsPausedFromWater;
    float yieldReductionFromWater;
    bool isDead;
    bool isInDrought;
    int timesWateredToday = 0;
    GameObject currentPest;
    float passivePestPreventionPercent = 0f;
    int passivePestPreventionDuration = 0;
    int daysWithPests;
    bool growthIsPausedFromPest;
    float yieldReductionFromPest;
    float CurrentYield => 100f - yieldReductionFromWater - yieldReductionFromPest;

    bool shouldDoWaterCheck = true;

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
        EventBus.Instance?.AddEventListener(GameEventType.PestWateredEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.CropHarvestedEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.PestDisasterEventStart, this);
        EventBus.Instance?.AddEventListener(GameEventType.CropPassivePestAppliedEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.PestPassivePestAppliedEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.CropActivePestAppliedEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.PestActivePestAppliedEvent, this);
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
        yieldReductionFromWater = 0f;
        yieldReductionFromPest = 0f;
        daysWithoutWater = 0; // 0 because then sunny day gets called and sets it to 1
        timesWateredToday = 0;
        daysWithPests = 0;
        acceptedItems.Add(startingSeed, 1);
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex == TrailerSceneManager.MAIN_SCENE_INDEX)
            InputManagerScript.Instance.AddTrailerHotkeyAction(2, ToggleShouldDoWaterCheck);
        else if (sceneIndex == TrailerSceneManager.CROP_SCENE_INDEX)
        {
            InputManagerScript.Instance.AddTrailerHotkeyAction(4, ResetGrowthStage);
            InputManagerScript.Instance.AddTrailerHotkeyAction(5, GoToNextStage);
        }
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener(GameEventType.NewDayEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.ClearSkyWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.RainWeatherEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventStart, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.DroughtDisasterEventEnd, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.PlantWateredEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.PestWateredEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.CropHarvestedEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.PestDisasterEventStart, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.CropPassivePestAppliedEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.PestPassivePestAppliedEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.CropActivePestAppliedEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.PestActivePestAppliedEvent, this);
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex == TrailerSceneManager.MAIN_SCENE_INDEX)
            InputManagerScript.Instance.RemoveTrailerHotkeyAction(2, ToggleShouldDoWaterCheck);
        else if (sceneIndex == TrailerSceneManager.CROP_SCENE_INDEX)
        {
            InputManagerScript.Instance.RemoveTrailerHotkeyAction(4, ResetGrowthStage);
            InputManagerScript.Instance.RemoveTrailerHotkeyAction(5, GoToNextStage);
        }
    }

    void ToggleShouldDoWaterCheck(InputAction.CallbackContext ctx) => shouldDoWaterCheck = !shouldDoWaterCheck;

    void ResetGrowthStage(InputAction.CallbackContext ctx)
    {
        currentGrowCycle = 1;
        if (currentCropPrefab != null) Destroy(currentCropPrefab);
        currentCropPrefab = Instantiate(currentCrop.GrowthStages[currentGrowCycle - 1].aliveCrop.cropPrefab, spawnPosition.position, spawnPosition.rotation);
    }

    void GoToNextStage(InputAction.CallbackContext ctx)
    {
        if (currentCropPrefab != null) Destroy(currentCropPrefab);
        if (currentGrowCycle == currentCrop.GrowthStages.Length) currentCropPrefab = Instantiate(currentCrop.GrowthStages[currentGrowCycle - 1].deadCrop.cropPrefab, spawnPosition.position, spawnPosition.rotation);
        else
        {
            ++currentGrowCycle;
            currentCropPrefab = Instantiate(currentCrop.GrowthStages[currentGrowCycle - 1].aliveCrop.cropPrefab, spawnPosition.position, spawnPosition.rotation);
        }
    }

    protected override bool ExtraAddLogic(ItemScriptableObject item)
    {
        if (item is SeedScribtableObject seed)
        {
            if (currentCapacity > 0 || !IsTilled) return false;
            currentCrop = seed.CropToPlant;
            currentGrowCycle = 0;
            daysWithoutWater = 1;
            growthIsPausedFromWater = false;
            yieldReductionFromWater = 0f;
            yieldReductionFromPest = 0f;
            isDead = false;
            daysWithPests = 0;
            return true;
        }
        return false;
    }

    protected override void ExtraClearLogic()
    {
        currentCrop = null;
        IsTilled = false;
        Destroy(currentCropPrefab);
        currentCropPrefab = null;
        if (currentPest != null)
        {
            Destroy(currentPest);
            currentPest = null;
        }
    }

    public void WaterPlant()
    {
        ++timesWateredToday;
        if (timesWateredToday < (isInDrought && (currentCrop?.IsSusceptibleToDrought ?? true) ? waterNeededPerDayInDrought : waterNeededPerDay)) return;
        daysWithoutWater = 0;
        growthIsPausedFromWater = false;
        IsWet = true;
    }

    public void TillSoil() => IsTilled = true;

    public void ApplyPassivePesticide(float percentReduction, int duration)
    {
        if (percentReduction <= 0 || duration <= 0) return;
        passivePestPreventionPercent = percentReduction;
        passivePestPreventionDuration = duration; // Reset duration to make
    }

    public void ApplyActivePesticide()
    {
        if (currentPest != null)
        {
            Destroy(currentPest);
            currentPest = null;
            daysWithPests = 0;
            growthIsPausedFromPest = false;
        }
    }

    void UpdateMaterial() => _renderer.material = IsTilled ? (IsWet ? wetFarmlandMaterial : farmlandMaterial) : (IsWet ? wetSoilMaterial : soilMaterial);

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.NewDayEvent:
                if (passivePestPreventionDuration > 0) --passivePestPreventionDuration;
                if (passivePestPreventionDuration <= 0) passivePestPreventionPercent = 0f;
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
            case GameEventType.PlantWateredEvent: // Can water farm tile, crop or pest
                if (parameters[0] is GameObject wateredCrop && wateredCrop == currentCropPrefab) WaterPlant();
                break;
            case GameEventType.PestWateredEvent: // Can water farm tile, crop or pest
                if (parameters[0] is GameObject wateredPest && wateredPest == currentPest) WaterPlant();
                break;
            case GameEventType.CropHarvestedEvent:
                if (parameters[0] is GameObject harvestedCrop && harvestedCrop == currentCropPrefab)
                {
                    EventBus.Instance?.BroadcastEvent(GameEventType.GainCropEvent, currentCrop, isDead, currentGrowCycle, CurrentYield);
                    ClearItems();
                }
                break;
            case GameEventType.PestDisasterEventStart:
                SpawnPest(WorldManager.Instance?.CurrentDifficulty ?? Difficulty.Easy);
                break;
            case GameEventType.CropPassivePestAppliedEvent: // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject passivePestCrop && passivePestCrop == currentCropPrefab)
                {
                    if (parameters[1] is float percentReduction && parameters[2] is int duration) ApplyPassivePesticide(percentReduction, duration);
                }
                break;
            case GameEventType.PestPassivePestAppliedEvent: // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject passivePestPest && passivePestPest == currentPest)
                {
                    if (parameters[1] is float percentReduction && parameters[2] is int duration) ApplyPassivePesticide(percentReduction, duration);
                }
                break;
            case GameEventType.CropActivePestAppliedEvent: // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject activePestCrop && activePestCrop == currentCropPrefab) ApplyActivePesticide();
                break;
            case GameEventType.PestActivePestAppliedEvent: // Can apply to farm tile, crop or pest
                if (parameters[0] is GameObject activePestPest && activePestPest == currentPest) ApplyActivePesticide();
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
        CheckForPestSetbacks();
        if (currentCrop == null) return;
        if (!isDead && !growthIsPausedFromWater && !growthIsPausedFromPest) currentGrowCycle = Mathf.Clamp(++currentGrowCycle, 1, currentCrop.GrowthStages.Length);
        else if (currentGrowCycle == 0) ++currentGrowCycle;
        if (currentCropPrefab != null) Destroy(currentCropPrefab);
        GameObject cropPrefab = isDead ? currentCrop.GrowthStages[currentGrowCycle - 1].deadCrop.cropPrefab : currentCrop.GrowthStages[currentGrowCycle - 1].aliveCrop.cropPrefab;
        currentCropPrefab = Instantiate(cropPrefab, spawnPosition.position, spawnPosition.rotation);
    }

    void CheckForWaterSetbacks()
    {
        if (!shouldDoWaterCheck) return;
        if (daysWithoutWater == 0) growthIsPausedFromWater = false;
        foreach (LackOfWaterSetback lackOfWaterSetback in currentCrop.LackOfWaterSetbacks)
        {
            if (daysWithoutWater < lackOfWaterSetback.daysWithoutWater) continue;
            float chance = lackOfWaterSetback.chance / 100f;
            if (chance <= TOLERANCE) continue; // ~0%
            else if (chance < 1f - TOLERANCE && Random.value > chance) continue; // Less than ~100% and failed roll
            if (lackOfWaterSetback.type == LackOfWaterSetbackType.GrowthPause) growthIsPausedFromWater = true;
            else if (lackOfWaterSetback.type == LackOfWaterSetbackType.YieldReduction) yieldReductionFromWater = Mathf.Max(yieldReductionFromWater, lackOfWaterSetback.strength);
            else
            {
                isDead = true;
                return;
            }
        }
    }

    void CheckForPestSetbacks()
    {
        if (currentPest == null)
        {
            daysWithPests = 0;
            growthIsPausedFromPest = false;
            return;
        }
        ++daysWithPests;
        if (daysWithPests >= 1) growthIsPausedFromPest = true;
        if (daysWithPests >= 2) yieldReductionFromPest = Mathf.Max(yieldReductionFromPest, 20f);
        if (daysWithPests >= 3) isDead = true;
    }

    void SpawnPest(Difficulty currentDifficulty = Difficulty.Easy)
    {
        if (currentCrop == null) return;
        float susceptibility = Mathf.Clamp01(currentCrop.SusceptibilityToPests / 100f);

        float deviation = Mathf.Max(Random.Range(1f - DEVIATION, 1f + DEVIATION), TOLERANCE); // Ensure the value > 0 for multiplication
        float difficultyModifier = Mathf.Max(difficultyModifiers[currentDifficulty], TOLERANCE); // Ensure the value > 0 for multiplication

        float passivePrevention = passivePestPreventionDuration > 0 ? passivePestPreventionPercent / 100f : 0f;

        float totalSusceptibility = Mathf.Clamp01(susceptibility * deviation * difficultyModifier - passivePrevention);
        if (totalSusceptibility <= TOLERANCE) return; // ~0%
        else if (totalSusceptibility < 1f - TOLERANCE && Random.value > totalSusceptibility) return; // Less than ~100% and failed roll
        if (currentPest == null) currentPest = Instantiate(pestPrefab, spawnPosition.position, spawnPosition.rotation);
    }
}
