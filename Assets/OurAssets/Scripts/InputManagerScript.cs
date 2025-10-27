using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerScript : MonoBehaviour
{
    public static InputManagerScript Instance { get; private set; }

    [Header("Player Actions")]
    [SerializeField]
    InputActionReference moveAction;
    [SerializeField]
    InputActionReference lookAction;
    [SerializeField]
    InputActionReference jumpAction;
    [SerializeField]
    InputActionReference interactAction;
    [SerializeField]
    InputActionReference pauseAction;
    [SerializeField]
    InputActionReference useAction;

    public InputDevice CurrentInputDevice { get; private set; }
    public InputDevice CurrentLookDevice { get; private set; }

    public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
    public Vector2 LookInput => lookAction.action.ReadValue<Vector2>();
    public void AddJumpAction(System.Action<InputAction.CallbackContext> action) => jumpAction.action.started += action;
    public void RemoveJumpAction(System.Action<InputAction.CallbackContext> action) => jumpAction.action.started -= action;
    public void AddInteractAction(System.Action<InputAction.CallbackContext> action) => interactAction.action.started += action;
    public void RemoveInteractAction(System.Action<InputAction.CallbackContext> action) => interactAction.action.started -= action;
    public void AddPauseAction(System.Action<InputAction.CallbackContext> action) => pauseAction.action.started += action;
    public void RemovePauseAction(System.Action<InputAction.CallbackContext> action) => pauseAction.action.started -= action;
    public void AddUseAction(System.Action<InputAction.CallbackContext> action) => useAction.action.started += action;
    public void RemoveUseAction(System.Action<InputAction.CallbackContext> action) => useAction.action.started -= action;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        moveAction.action.performed += DetectInputDevice;
        lookAction.action.performed += DetectInputDevice;
        lookAction.action.performed += DetectLookDevice;
        jumpAction.action.performed += DetectInputDevice;
        interactAction.action.performed += DetectInputDevice;
        pauseAction.action.performed += DetectInputDevice;
        useAction.action.performed += DetectInputDevice;
        if (!moveAction.action.enabled) moveAction.action.Enable();
        if (!lookAction.action.enabled) lookAction.action.Enable();
        if (!jumpAction.action.enabled) jumpAction.action.Enable();
        if (!interactAction.action.enabled) interactAction.action.Enable();
        if (!pauseAction.action.enabled) pauseAction.action.Enable();
        if (!useAction.action.enabled) useAction.action.Enable();
    }

    void OnDestroy()
    {
        moveAction.action.performed -= DetectInputDevice;
        lookAction.action.performed -= DetectInputDevice;
        lookAction.action.performed -= DetectLookDevice;
        jumpAction.action.performed -= DetectInputDevice;
        interactAction.action.performed -= DetectInputDevice;
        pauseAction.action.performed -= DetectInputDevice;
        useAction.action.performed -= DetectInputDevice;
    }

    void DetectInputDevice(InputAction.CallbackContext ctx) => CurrentInputDevice = ctx.control.device;

    void DetectLookDevice(InputAction.CallbackContext ctx) => CurrentLookDevice = ctx.control.device;
}
