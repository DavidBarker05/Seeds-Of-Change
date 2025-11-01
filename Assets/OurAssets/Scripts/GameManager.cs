using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour, IEventListener
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    bool canPauseInCurrentScene = false;
    [SerializeField]
    bool startCurrentSceneFocused = false;
    [SerializeField]
    GameObject pauseBackground;
    [SerializeField]
    GameObject pauseMenu;
    [SerializeField]
    GameObject winMenu;
    [SerializeField]
    LoseMenu loseMenu;

    bool _isPaused = false;
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            _isPaused = canPauseInCurrentScene && value;
            Time.timeScale = canPauseInCurrentScene && _isPaused ? 0f : 1f;
            if (_isPaused) EnableMouse();
            else DisableMouse();
            if (pauseBackground != null) pauseBackground.SetActive(_isPaused);
            if (!_isPaused) pauseButtonMadePaused = false;
        }
    }

    bool pauseButtonMadePaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        EventBus.Instance?.AddEventListener(GameEventType.GameWinEvent, this);
        EventBus.Instance?.AddEventListener(GameEventType.GameLoseEvent, this);
    }

    void Start()
    {
        InputManagerScript.Instance?.AddPauseAction(TogglePause);
        if (startCurrentSceneFocused) DisableMouse();
        else EnableMouse();
    }

    void OnDestroy()
    {
        IsPaused = false;
        EnableMouse();
        InputManagerScript.Instance?.RemovePauseAction(TogglePause);
        EventBus.Instance?.RemoveEventListener(GameEventType.GameWinEvent, this);
        EventBus.Instance?.RemoveEventListener(GameEventType.GameLoseEvent, this);
    }

    void TogglePause(InputAction.CallbackContext ctx)
    {
        if (IsPaused && !pauseButtonMadePaused) return;
        pauseButtonMadePaused = true;
        IsPaused = !IsPaused;
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    public void DisableMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void EnableMouse()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void OnEventReceived(GameEventType eventType, params object[] parameters)
    {
        switch (eventType)
        {
            case GameEventType.GameWinEvent:
                if (winMenu == null) return;
                IsPaused = true;
                winMenu.SetActive(true);
                break;
            case GameEventType.GameLoseEvent:
                if (loseMenu == null) return;
                if (parameters[0] is int familyFood && parameters[1] is int communityFood)
                {
                    IsPaused = true;
                    loseMenu.UpdateLoseText(familyFood, communityFood);
                    loseMenu.gameObject.SetActive(true);
                }
                break;
            default:
                break;
        }
    }
}
