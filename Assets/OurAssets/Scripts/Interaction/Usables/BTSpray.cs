using UnityEngine;
using UnityEngine.Serialization;

public class BTSpray : Usable
{

    [SerializeField, Min(0), FormerlySerializedAs("numberOfUses")]
    int m_NumberOfUses = 10;

    int m_CurrentNumberOfUses;

    new void Awake()
    {
        base.Awake();
        m_CurrentNumberOfUses = m_NumberOfUses;
    }

    public override UseInfo Use(Interactable targetInteractable)
    {
        if (targetInteractable is FarmTile farmTile)
        {
            farmTile.ApplyActivePesticide();
            --m_CurrentNumberOfUses;
        }
        else if (targetInteractable is CropScript crop)
        {
            EventBus.Instance?.BroadcastEvent("CropActivePestAppliedEvent", crop.gameObject);
            --m_CurrentNumberOfUses;
        }
        else if (targetInteractable is PestScript pest)
        {
            EventBus.Instance?.BroadcastEvent("PestActivePestAppliedEvent", pest.gameObject);
            --m_CurrentNumberOfUses;
        }
        return new UseInfo()
        {
            FinishedUsing = m_CurrentNumberOfUses <= 0
        };
    }
}
