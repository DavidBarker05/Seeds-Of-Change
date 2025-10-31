using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour, IEventListener
{
    public static ItemSpawner Instance { get; private set; }

    [SerializeField]
    List<ItemObjectPair> itemObjectPairs = new List<ItemObjectPair>();
    [SerializeField]
    Transform spawnPoint;
    [SerializeField, Min(0f)]
    float spawnTime = 0.1f;
    [SerializeField, Range(0f, 0.5f)]
    float spawnRadius = 0.2f;

    Dictionary<ItemScriptableObject, GameObject> itemObjectPairDictionary = new Dictionary<ItemScriptableObject, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        foreach (ItemObjectPair itemObjectPair in itemObjectPairs)
        {
            if (itemObjectPair.item != null && itemObjectPair.itemObject != null && !itemObjectPairDictionary.ContainsKey(itemObjectPair.item)) itemObjectPairDictionary.Add(itemObjectPair.item, itemObjectPair.itemObject);
        }
    }

    void Start() => EventBus.Instance?.AddEventListener(GameEventType.ItemPurchasedEvent, this);

    void OnDestroy() => EventBus.Instance?.RemoveEventListener(GameEventType.ItemPurchasedEvent, this);

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.ItemPurchasedEvent:
                if (parameters[0] is ItemScriptableObject item && itemObjectPairDictionary.ContainsKey(item) && parameters[1] is int amount) StartCoroutine(SpawnObject(itemObjectPairDictionary[item], amount));
                break;
            default:
                break;
        }
    }

    System.Collections.IEnumerator SpawnObject(GameObject itemObject, int amount)
    {
        for (int i = 0; i < amount; ++i)
        {
            yield return new WaitForSeconds(spawnTime);
            Vector3 offset = Random.insideUnitSphere * spawnRadius;
            if (spawnPoint != null) Instantiate(itemObject, spawnPoint.position + offset, spawnPoint.rotation);
        }
    }
}

[System.Serializable]
public struct ItemObjectPair
{
    public ItemScriptableObject item;
    public GameObject itemObject;
}
