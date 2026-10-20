using UnityEngine;
using Util.SystemUtils;

public class PausePlayerCharacterInitData : IPlayerCharacterInitData
{
    public Player Player { get; set; }
}

public class PausePlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
}

public class PausePlayerCharacter : PlayerCharacter
{
    [SerializeField]
    MenuHandler m_PauseScreen;

    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => "PausePlayer";
    public override bool MouseVisible => true;
    public override bool DoCameraRotation => false;

    public bool Paused { get; private set; } = false;

    Player m_Player;
    PlayerCharacter m_LastCharacter = null;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        PausePlayerCharacterInitData initData = Sys.AssertType<PausePlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_Player = initData.Player;
        HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "PausePlayerCharacter hasn't been initialised!");
        Sys.AssertType<PausePlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
    }

    public override void OnPausePressed() => ResumeGame();

    public void PauseGame(PlayerCharacter characterToSwitchBackTo)
    {
        Sys.Assert(HasBeenInitialised, "PausePlayerCharacter hasn't been initialised!");
        if (!m_PauseScreen)
        {
            Debug.LogWarning("Pause screen is missing");
            return;
        }
        if (Paused || Time.timeScale == 0f || characterToSwitchBackTo == null) return;
        Paused = true;
        m_LastCharacter = characterToSwitchBackTo;
        CameraTarget = m_LastCharacter.CameraTarget;
        m_Player.ChangeCharacter(this);
        m_PauseScreen.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        Sys.Assert(HasBeenInitialised, "PausePlayerCharacter hasn't been initialised!");
        if (!m_PauseScreen)
        {
            Debug.LogWarning("Pause screen is missing");
            return;
        }
        if (!Paused || Time.timeScale == 1f || m_LastCharacter == null) return;
        Paused = false;
        m_Player.ChangeCharacter(m_LastCharacter);
        m_LastCharacter = null;
        CameraTarget = null;
        m_PauseScreen.Close();
        m_PauseScreen.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}