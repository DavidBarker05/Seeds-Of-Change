using UnityEngine;
using Util.SystemUtils;

public class MenuPlayerCharacterInitData : IPlayerCharacterInitData
{
    public Player Player { get; set; }
}

public class MenuPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
}

public class MenuPlayerCharacter : PlayerCharacter
{
    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => string.Empty;
    public override bool MouseVisible => true;
    public override bool DoCameraRotation => false;

    Player m_Player;
    PlayerCharacter m_LastCharacter;
    GameObject m_LastUI;
    GameObject m_CurrentUI;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        MenuPlayerCharacterInitData initData = Sys.AssertType<MenuPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_Player = initData.Player;
        HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "MenuPlayerCharacter hasn't been initialised");
        Sys.AssertType<MenuPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
    }

    public override void OnPausePressed() => Sys.Assert(HasBeenInitialised, "MenuPlayerCharacter hasn't been initialised");

    public void OnMenuOpen(PlayerCharacter currentCharacter, GameObject currentUI, GameObject newUI)
    {
        Sys.Assert(HasBeenInitialised, "MenuPlayerCharacter hasn't been initialised");
        if (m_LastCharacter || !currentCharacter || !newUI) return;
        CameraTarget = currentCharacter.CameraTarget;
        m_LastCharacter = currentCharacter;
        m_Player.ChangeCharacter(this);
        m_LastUI = currentUI;
        m_CurrentUI = newUI;
        m_LastUI?.SetActive(false);
        m_CurrentUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnMenuExit()
    {
        Sys.Assert(HasBeenInitialised, "MenuPlayerCharacter hasn't been initialised");
        if (!m_LastCharacter) return;
        Time.timeScale = 1f;
        m_Player.ChangeCharacter(m_LastCharacter);
        m_LastUI?.SetActive(true);
        m_CurrentUI.SetActive(false);
        CameraTarget = null;
        m_LastCharacter = null;
        m_LastUI = null;
        m_CurrentUI = null;
    }
}