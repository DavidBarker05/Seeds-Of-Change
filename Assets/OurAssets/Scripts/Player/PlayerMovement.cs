using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(1f)]
    float movementSpeed = 4.5f;
    [SerializeField, Range(0f, 0.1f)]
    float deadZone = 0.05f;
    [SerializeField, Min(0f)]
    float groundDistance = 0.1f;
    [SerializeField]
    LayerMask groundMask;
    [SerializeField, Min(0f)]
    float jumpHeight = 1f;

    CharacterController cc;
    float vVel;
    bool isGrounded;

    const float GRAVITY = -9.81f;

    void Awake() =>  cc = GetComponent<CharacterController>();

    private void Start() => InputManagerScript.Instance?.AddJumpAction(Jump);

    void Update()
    {
        if (GameManager.Instance?.IsPaused ?? false) return;
        isGrounded = Physics.SphereCast(transform.position + Vector3.up * cc.radius, cc.radius, Vector3.down, out RaycastHit hit, groundDistance, groundMask)
            && (!hit.collider.TryGetComponent<Holdable>(out Holdable h) || h.IsGrounded);
        float xIn = InputManagerScript.Instance?.MoveInput.x ?? 0f;
        float zIn = InputManagerScript.Instance?.MoveInput.y ?? 0f;
        Vector3 hIn = (xIn * transform.right + zIn * transform.forward).normalized;
        hIn = hIn.magnitude > deadZone ? hIn : Vector3.zero;
        Vector3 hVel = hIn * movementSpeed;
        if (isGrounded && vVel < 0f) vVel = -1f;
        vVel += GRAVITY * Time.deltaTime;
        Vector3 movement = new Vector3(hVel.x, vVel, hVel.z);
        cc.Move(movement * Time.deltaTime);
    }

    void OnDestroy() => InputManagerScript.Instance?.RemoveJumpAction(Jump);

    void Jump(InputAction.CallbackContext obj)
    {
        if (isGrounded) vVel = Mathf.Sqrt(2f * -GRAVITY * jumpHeight);
    }
}
