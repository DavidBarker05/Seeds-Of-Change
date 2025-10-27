using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    [Header("Input Actions")]
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

    public InputDevice CurrentInputDevice { get; private set; } // This will be used to display on screen controls if I have time
    public InputDevice CurrentLookDevice { get; private set; } // This is used for look sensitivity
    public InputDevice CurrentPauseDevice { get; private set; } // This will be used unhide the mouse if used keyboard to pause or keep mouse hidden until moved if used a controller (if I have time to add controller menu functionality)

    public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
    public Vector2 LookAction => lookAction.action.ReadValue<Vector2>();
    public void AddJumpAction(System.Action<InputAction.CallbackContext> action) => jumpAction.action.started += action;
    public void RemoveJumpAction(System.Action<InputAction.CallbackContext> action) => jumpAction.action.started -= action;
    public void AddInteractAction(System.Action<InputAction.CallbackContext> action) => interactAction.action.started += action;
    public void RemoveInteractAction(System.Action<InputAction.CallbackContext> action) => interactAction.action.started -= action;
    public void AddPauseAction(System.Action<InputAction.CallbackContext> action) => interactAction.action.started += action;
    public void RemovePauseAction(System.Action<InputAction.CallbackContext> action) => interactAction.action.started -= action;

    void Awake()
    {
        moveAction.action.performed += DetectInputDevice;
        lookAction.action.performed += DetectLookDevice;
        lookAction.action.performed += DetectInputDevice;
        jumpAction.action.performed += DetectInputDevice;
        interactAction.action.performed += DetectInputDevice;
        pauseAction.action.performed += DetectInputDevice;
        pauseAction.action.performed += DetectPauseDevice;
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        interactAction.action.Enable();
        pauseAction.action.Enable();
    }

    void OnDestroy()
    {
        moveAction.action.performed -= DetectInputDevice;
        lookAction.action.performed -= DetectLookDevice;
        lookAction.action.performed -= DetectInputDevice;
        jumpAction.action.performed -= DetectInputDevice;
        interactAction.action.performed -= DetectInputDevice;
        pauseAction.action.performed -= DetectInputDevice;
        pauseAction.action.performed -= DetectPauseDevice;
    }

    void DetectInputDevice(InputAction.CallbackContext ctx) => CurrentInputDevice = ctx.control.device;

    void DetectLookDevice(InputAction.CallbackContext ctx) => CurrentLookDevice = ctx.control.device;

    void DetectPauseDevice(InputAction.CallbackContext ctx) => CurrentPauseDevice = ctx.control.device;
}
