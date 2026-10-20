using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour, IEventListener
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    MenuPlayerCharacter m_MenuPlayerCharacter;
    [SerializeField]
    FirstPersonPlayerCharacter m_FirstPersonPlayerCharacter;
    [SerializeField]
    GameObject m_HUD;
    [SerializeField, FormerlySerializedAs("winMenu")]
    GameObject m_WinMenu;
    [SerializeField, FormerlySerializedAs("loseMenu")]
    LoseMenu m_LoseMenu;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            EventBus.Instance?.AddEventListener("GameWinEvent", this);
            EventBus.Instance?.AddEventListener("GameLoseEvent", this);
        }
    }

    void OnDestroy()
    {
        EventBus.Instance?.RemoveEventListener("GameWinEvent", this);
        EventBus.Instance?.RemoveEventListener("GameLoseEvent", this);
    }

    public void OnEventReceived(string eventName, params object[] parameters)
    {
        if (eventName == "GameWinEvent") m_MenuPlayerCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_WinMenu);
        else if (eventName == "GameLoseEvent")
        {
            if (parameters[0] is int familyFood && parameters[1] is int communityFood)
            {
                m_LoseMenu.UpdateLoseText(familyFood, communityFood);
                m_MenuPlayerCharacter.OnMenuOpen(m_FirstPersonPlayerCharacter, m_HUD, m_LoseMenu.gameObject);
            }
        }
    }
}
