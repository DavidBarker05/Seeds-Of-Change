using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CropScriptableObject", menuName = "Scriptable Objects/CropScriptableObject")]
public class CropScriptableObject : ScriptableObject
{
    [field: SerializeField]
    public string CropName { get; private set; }
    [field: SerializeField]
    public GrowthStage[] GrowthStages { get; private set; }
    [field: SerializeField]
    public LackOfWaterSetback[] LackOfWaterSetbacks { get; private set; }
    [field: SerializeField]
    public bool IsSusceptibleToDrought { get; private set; }
    [field: SerializeField, Min(0f)]
    public float SusceptibilityToPests { get; private set; }
}

[Serializable]
public class CropYield
{
    public ItemScriptableObject yieldItem;
    public int yieldAmount;
}

[Serializable]
public class CropVariant
{
    public GameObject cropPrefab;
    public CropYield cropYield;
}

[Serializable]
public class GrowthStage
{
    public CropVariant aliveCrop;
    public CropVariant deadCrop;
}

[Serializable]
public enum LackOfWaterSetbackType
{
    GrowthPause,
    YieldReduction,
    Death
}

[Serializable]
public class LackOfWaterSetback
{
    public int daysWithoutWater;
    public LackOfWaterSetbackType type;
    public float chance;
    public float strength;
}
