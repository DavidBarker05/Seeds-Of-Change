using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CropScriptableObject", menuName = "Scriptable Objects/CropScriptableObject")]
public class CropScriptableObject : ScriptableObject
{
    [SerializeField]
    string cropName;
    [SerializeField]
    GrowthStage[] growthStages;
    [SerializeField]
    LackOfWaterSetback[] lackOfWaterSetbacks;
    [SerializeField]
    bool isSusceptibleToDrought;

    public string CropName => cropName;
    public GrowthStage[] GrowthStages => (GrowthStage[])growthStages.Clone();
    public LackOfWaterSetback[] LackOfWaterSetbacks => (LackOfWaterSetback[])lackOfWaterSetbacks.Clone();
    public bool IsSusceptibleToDrought => isSusceptibleToDrought;
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
