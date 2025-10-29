using UnityEngine;

public class SeedPacket : Usable
{
    SeedScribtableObject _seedData;
    public SeedScribtableObject SeedData
    {
        get => _seedData;
        set
        {
            _seedData = value;
        }
    }

    int currentNumberOfUses;

    public override bool Use(Interactable targetInteractable)
    {
        if (targetInteractable == null)
        {
            #if UNITY_EDITOR
                Debug.LogWarning("Seed packet needs to be used on farm land, you used it on nothing");
            #endif
        }
        else
        {
            if (targetInteractable is FarmTile farmTile)
            {

            }
        }
        return currentNumberOfUses > 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
