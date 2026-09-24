using UnityEngine;
using UnityEngine.InputSystem;

public class CropVideoManager : MonoBehaviour
{
    [SerializeField]
    CameraDolly m_Dolly;
    [SerializeField]
    FarmTile m_Crop; // call AdvanceStage() to grow
    [SerializeField]
    SeedScribtableObject m_CowpeaSeed;
    [SerializeField]
    SeedScribtableObject m_SorghumSeed;
    [SerializeField]
    SeedScribtableObject m_TomatoSeed;

    int m_GrowthStageCount;
    int m_StagesTriggered;

    void Start()
    {
        InputManagerScript.Instance.AddTrailerHotkeyAction(0, DoResetDolly);
        InputManagerScript.Instance.AddTrailerHotkeyAction(1, DoStartDolly);
        InputManagerScript.Instance.AddTrailerHotkeyAction(2, SetSeedToCowpea);
        InputManagerScript.Instance.AddTrailerHotkeyAction(3, SetSeedToSorghum);
        InputManagerScript.Instance.AddTrailerHotkeyAction(4, SetSeedToTomato);
    }

    void OnDestroy()
    {
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(0, DoResetDolly);
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(1, DoStartDolly);
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(2, SetSeedToCowpea);
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(3, SetSeedToSorghum);
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(4, SetSeedToTomato);
    }

    void Update()
    {
        if (!m_Dolly.IsPlaying) return;
        m_Dolly.UpdateDolly(Time.deltaTime);
        if (m_GrowthStageCount <= 0) return;
        float elapsed = m_Dolly.CurrentTime - m_Dolly.StartTime;
        int totalIntervals = m_GrowthStageCount + 1;
        float intervalDuration = m_Dolly.MovementDuration / totalIntervals;
        int expectedStagesByNow = Mathf.Min(m_GrowthStageCount, Mathf.FloorToInt(elapsed / intervalDuration));
        while (m_StagesTriggered < expectedStagesByNow)
        {
            m_Crop.AdvanceStage();
            m_StagesTriggered++;
        }
    }

    void DoResetDolly(InputAction.CallbackContext ctx)
    {
        m_Dolly.ResetDolly();
        m_StagesTriggered = 0;
    }

    void DoStartDolly(InputAction.CallbackContext ctx)
    {
        m_GrowthStageCount = m_Crop.GrowthTransitionsToFullyGrown;
        m_StagesTriggered = 0;
        m_Dolly.StartDolly();
    }

    void SetSeedToCowpea(InputAction.CallbackContext ctx) => ResetSeed(m_CowpeaSeed);
    void SetSeedToSorghum(InputAction.CallbackContext ctx) => ResetSeed(m_SorghumSeed);
    void SetSeedToTomato(InputAction.CallbackContext ctx) => ResetSeed(m_TomatoSeed);

    void ResetSeed(SeedScribtableObject seed)
    {
        m_Crop.SetSeed(seed);
        m_StagesTriggered = 0;
    }
}