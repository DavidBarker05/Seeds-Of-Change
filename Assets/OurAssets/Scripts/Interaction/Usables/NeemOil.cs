using UnityEngine;
using UnityEngine.Serialization;

public class NeemOil : Usable
{
    [SerializeField, Min(0), FormerlySerializedAs("numberOfUses")]
    int m_NumberOfUses = 10;
    [SerializeField, Min(0f), FormerlySerializedAs("percentReduction")]
    float m_PercentReduction = 50f;
    [SerializeField, Min(0), FormerlySerializedAs("duration")]
    int m_Duration = 5;

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
            farmTile.ApplyPassivePesticide(m_PercentReduction, m_Duration);
            --m_CurrentNumberOfUses;
        }
        else if (targetInteractable is CropScript crop)
        {
            EventBus.Instance?.BroadcastEvent("CropPassivePestAppliedEvent", crop.gameObject, m_PercentReduction, m_Duration);
            --m_CurrentNumberOfUses;
        }
        else if (targetInteractable is PestScript pest)
        {
            EventBus.Instance?.BroadcastEvent("PestPassivePestAppliedEvent", pest.gameObject, m_PercentReduction, m_Duration);
            --m_CurrentNumberOfUses;
        }
        return new UseInfo()
        {
            FinishedUsing = m_CurrentNumberOfUses <= 0
        };
    }
}
