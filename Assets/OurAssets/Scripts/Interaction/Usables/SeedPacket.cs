using UnityEngine;

public class SeedPacket : Usable
{
    [field: SerializeField]
    public SeedScribtableObject SeedData { get; private set; }

    int currentNumberOfUses;

    new void Awake()
    {
        base.Awake();
        currentNumberOfUses = SeedData?.NumberOfUses ?? 3;
    }

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
                if (!farmTile.Interact(SeedData, 1).DoEndInteraction) --currentNumberOfUses;
            }
        }
        return currentNumberOfUses <= 0;
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
