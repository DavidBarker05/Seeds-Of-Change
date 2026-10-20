using UnityEngine;

public class SeedPacket : Usable
{
    [field: SerializeField]
    public SeedScribtableObject SeedData { get; private set; }

    int m_CurrentNumberOfUses;

    new void Awake()
    {
        base.Awake();
        m_CurrentNumberOfUses = SeedData.NumberOfUses;
    }

    public override UseInfo Use(Interactable targetInteractable)
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
                if (farmTile.Interact(SeedData, 1).OutArguments[0] is AddItemInfo addItemInfo && addItemInfo.WasAdded) --m_CurrentNumberOfUses;
            }
        }
        return new UseInfo()
        {
            FinishedUsing = m_CurrentNumberOfUses <= 0
        };
    }
}
