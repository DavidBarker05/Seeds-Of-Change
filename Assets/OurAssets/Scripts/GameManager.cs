using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    bool canPauseInCurrentScene = false;
    [SerializeField]
    bool startCurrentScenePaused = false;
    [SerializeField]
    bool startCurrentSceneFocused = false;

    bool _isPaused = false;
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            Time.timeScale = canPauseInCurrentScene && value ? 0f : 1f;
            if (value) EnableMouse();
            else DisableMouse();
            _isPaused = canPauseInCurrentScene && value;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.canPauseInCurrentScene = canPauseInCurrentScene;
            Instance.startCurrentScenePaused = startCurrentScenePaused;
            Instance.startCurrentSceneFocused = startCurrentSceneFocused;
            Instance.IsPaused = startCurrentScenePaused;
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
        IsPaused = startCurrentScenePaused;
        if (startCurrentSceneFocused) DisableMouse();
        else EnableMouse();
    }

    void OnDestroy() => InputManagerScript.Instance?.RemovePauseAction(TogglePause);

    void TogglePause(InputAction.CallbackContext obj) => IsPaused = !IsPaused;

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
