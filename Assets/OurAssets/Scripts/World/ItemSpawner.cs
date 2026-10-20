using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemSpawner : MonoBehaviour, IEventListener
{
    public static ItemSpawner Instance { get; private set; }

    [SerializeField, FormerlySerializedAs("itemObjectPairs")]
    List<ItemObjectPair> m_ItemObjectPairs;
    [SerializeField, FormerlySerializedAs("spawnPoint")]
    Transform m_SpawnPoint;
    [SerializeField, Min(0f), FormerlySerializedAs("spawnTime")]
    float m_SpawnTime = 0.1f;
    [SerializeField, Range(0f, 0.5f), FormerlySerializedAs("spawnRadius")]
    float m_SpawnRadius = 0.2f;

    readonly Dictionary<ItemScriptableObject, GameObject> m_ItemObjectPairDictionary = new Dictionary<ItemScriptableObject, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            foreach (ItemObjectPair itemObjectPair in m_ItemObjectPairs)
            {
                if (itemObjectPair.Item != null && itemObjectPair.ItemObject != null && !m_ItemObjectPairDictionary.ContainsKey(itemObjectPair.Item)) m_ItemObjectPairDictionary.Add(itemObjectPair.Item, itemObjectPair.ItemObject);
            }
            EventBus.Instance?.AddEventListener("ItemSpawnEvent", this);
        }
    }

    void OnDestroy() => EventBus.Instance?.RemoveEventListener("ItemSpawnEvent", this);

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        if (eventName != "ItemSpawnEvent") return;
        if (parameters[0] is ItemScriptableObject item && m_ItemObjectPairDictionary.ContainsKey(item) && parameters[1] is int amount) StartCoroutine(SpawnObject(m_ItemObjectPairDictionary[item], amount));
    }

    System.Collections.IEnumerator SpawnObject(GameObject itemObject, int amount)
    {
        for (int i = 0; i < amount; ++i)
        {
            yield return new WaitForSeconds(m_SpawnTime);
            Vector3 offset = Random.insideUnitSphere * m_SpawnRadius;
            if (m_SpawnPoint != null) Instantiate(itemObject, m_SpawnPoint.position + offset, m_SpawnPoint.rotation);
        }
    }
}

[System.Serializable]
public struct ItemObjectPair
{
    [FormerlySerializedAs("item")]
    public ItemScriptableObject Item;
    [FormerlySerializedAs("itemObject")]
    public GameObject ItemObject;
}
