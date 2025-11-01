using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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
            pauseBackground.SetActive(_isPaused);
            if (!_isPaused) pauseButtonMadePaused = false;
        }
    }

    bool pauseButtonMadePaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.canPauseInCurrentScene = canPauseInCurrentScene;
            Instance.startCurrentSceneFocused = startCurrentSceneFocused;
            if (startCurrentSceneFocused) DisableMouse();
            else EnableMouse();
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        InputManagerScript.Instance?.AddPauseAction(TogglePause);
        if (startCurrentSceneFocused) DisableMouse();
        else EnableMouse();
    }

    void OnDestroy() => InputManagerScript.Instance?.RemovePauseAction(TogglePause);

    void TogglePause(InputAction.CallbackContext ctx)
    {
        if (IsPaused && !pauseButtonMadePaused) return;
        pauseButtonMadePaused = true;
        IsPaused = !IsPaused;
        pauseMenu.SetActive(true);
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
}
