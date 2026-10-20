using UnityEngine;
using Util.SystemUtils;

public class FirstPersonPlayerCharacterInitData : IPlayerCharacterInitData
{
    public CharacterSettings CharacterSettings { get; set; }
    public Player Player { get; set; }
    public PausePlayerCharacter PausePlayerCharacter { get; set; }
    public MenuPlayerCharacter MenuPlayerCharacter { get; set; }

    public Camera Camera { get; set; }
}

public class FirstPersonPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    #region Input
    public float DeltaTime { get; set; }

    public Vector2 MovementInput { get; set; }
    public bool JumpPressedThisFrame { get; set; }
    public bool SprintPressedThisFrame { get; set; }

    public bool InteractStarted { get; set; }
    public bool UseStarted { get; set; }
    #endregion Input

    #region Output
    public bool IsSprinting { get; set; }
    #endregion Output
}

[
    RequireComponent(
        typeof(CharacterController),
        typeof(PlayerInteraction),
        typeof(PlayerInventory)
    )
]
public class FirstPersonPlayerCharacter : PlayerCharacter
{
    static readonly float s_Epsilon = 0.05f;
    static readonly float s_SqrEpsilon = s_Epsilon * s_Epsilon;

    [SerializeField]
    Transform m_SpawnPoint;

    Camera m_Camera;
    CharacterSettings m_CharacterSettings;
    CharacterController m_CC;
    PlayerInteraction m_PI;

    bool m_bIsGrounded;
    bool m_bBumpedHead;

    Vector3 m_Velocity;
    bool m_bIsJumping;
    bool m_bIsFalling;

    float m_CurrentJumpBufferTimer;
    float m_CurrentCoyoteTimer;

    bool m_bIsSprinting;
    float m_MovementSpeed;

    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => "Player";
    public override bool MouseVisible => false;
    public override bool DoCameraRotation => true;

    PausePlayerCharacter m_PausePlayerCharacter;

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        FirstPersonPlayerCharacterInitData initData = Sys.AssertType<FirstPersonPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_CC = GetComponent<CharacterController>();
        m_PI = GetComponent<PlayerInteraction>();
        m_CharacterSettings = initData.CharacterSettings;
        m_PausePlayerCharacter = initData.PausePlayerCharacter;
        m_Camera = initData.Camera;
        m_PI.Init(
            new InteractionInitData()
            {
                Camera = m_Camera,
                CharacterController = m_CC,
                MenuPlayerCharacter = initData.MenuPlayerCharacter,
                FirstPersonPlayerCharacter = this
            }
        );
        Respawn();
        HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        FirstPersonPlayerCharacterUpdateData updateData = Sys.AssertType<FirstPersonPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
        HandleMovement(ref updateData);
        HandleInteraction(ref updateData);
        HandleUse(ref updateData);
        InteractionUpdateData interactionUpdateData = new InteractionUpdateData() { DeltaTime = updateData.DeltaTime };
        m_PI.UpdateInteraction(ref interactionUpdateData);
    }

    public override void OnPausePressed()
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        m_PausePlayerCharacter.PauseGame(this);
    }

    public void Respawn()
    {
        if (m_SpawnPoint == null) return;
        m_CC.enabled = false;
        transform.SetPositionAndRotation(m_SpawnPoint.position, m_SpawnPoint.rotation);
        m_CC.enabled = true;
        m_CC.Move(Vector3.down);
    }

    #region Movement
    void HandleMovement(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        UpdateRotation(m_Camera.transform.rotation);
        CollisionChecks();
        UpdateTimers(updateData.DeltaTime);
        UpdateMovementSpeed(updateData.SprintPressedThisFrame);
        UpdateHorizontalVelocity(updateData.MovementInput);
        JumpChecks(updateData.JumpPressedThisFrame);
        UpdateVerticalVelocity(updateData.DeltaTime);
        updateData.IsSprinting =
            m_bIsSprinting // Pressing sprint
            && (m_bIsGrounded || m_CharacterSettings.CanSprintInAir) // On ground or allowed to sprint in air
            && (updateData.MovementInput.x > s_Epsilon || updateData.MovementInput.y > s_Epsilon); // Pressing movement keys
        m_CC.Move(m_Velocity * updateData.DeltaTime);
    }

    void UpdateRotation(Quaternion rotation)
    {
        Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up).normalized;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    #region Collision Checks
    void GroundCheck() => m_bIsGrounded = m_CC.isGrounded;

    void BumpedHeadCheck() => m_bBumpedHead = m_CC.collisionFlags.HasFlag(CollisionFlags.Above);

    void CollisionChecks()
    {
        GroundCheck();
        BumpedHeadCheck();
    }
    #endregion Collision Checks

    void UpdateTimers(float deltaTime)
    {
        m_CurrentJumpBufferTimer -= deltaTime;
        if (!m_bIsGrounded) m_CurrentCoyoteTimer -= deltaTime;
        else m_CurrentCoyoteTimer = 0f;
    }

    void UpdateMovementSpeed(bool bSprintPressedThisFrame)
    {
        m_bIsSprinting = ((m_bIsGrounded && !m_bIsFalling) || m_CharacterSettings.CanSprintInAir) && bSprintPressedThisFrame;
        m_MovementSpeed = m_bIsSprinting ? m_CharacterSettings.SprintSpeed : m_CharacterSettings.MovementSpeed;
    }

    void UpdateHorizontalVelocity(Vector2 movementInput)
    {
        float xIn = movementInput.x;
        float zIn = movementInput.y;
        Vector3 hIn = Vector3.ClampMagnitude(xIn * transform.right + zIn * transform.forward, 1f);
        hIn = hIn.sqrMagnitude > s_SqrEpsilon ? hIn : Vector3.zero;
        m_Velocity.x = hIn.x * m_MovementSpeed;
        m_Velocity.z = hIn.z * m_MovementSpeed;
    }

    #region Jumping
    void JumpChecks(bool bJumpPressedThisFrame)
    {
        if (bJumpPressedThisFrame) m_CurrentJumpBufferTimer = m_CharacterSettings.JumpBuffer;
        if (m_CurrentJumpBufferTimer > 0f && !m_bIsJumping && (m_bIsGrounded || m_CurrentCoyoteTimer > 0f)) InitiateJump();
        if ((m_bIsJumping || m_bIsFalling) && m_bIsGrounded && m_Velocity.y <= 0f)
        {
            m_bIsJumping = false;
            m_bIsFalling = false;
            m_Velocity.y = 0f;
        }
    }

    void InitiateJump()
    {
        m_bIsJumping = true;
        m_CurrentJumpBufferTimer = 0f;
        m_Velocity.y = m_CharacterSettings.InitialJumpVelocity;
    }
    #endregion Jumping

    #region Update Vertical Velocity
    void UpdateVerticalVelocityWhileJumping(float deltaTime)
    {
        if (m_bBumpedHead && m_Velocity.y > 0) m_Velocity.y = 0f;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
        m_bIsFalling = m_Velocity.y < 0f;
    }

    void UpdateVerticalVelocityWhileFalling(float deltaTime)
    {
        m_bIsFalling = true;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
    }

    void UpdateVerticalVelocity(float deltaTime)
    {
        if (m_bIsJumping) UpdateVerticalVelocityWhileJumping(deltaTime);
        else if (!m_bIsGrounded) UpdateVerticalVelocityWhileFalling(deltaTime);
        else m_Velocity.y = -1f;
    }
    #endregion Update Vertical Velocity
    #endregion Movement

    #region Interaction
    void HandleInteraction(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (updateData.InteractStarted) m_PI.Interact();
        updateData.InteractStarted = false;
    }

    void HandleUse(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (updateData.UseStarted) m_PI.Use();
        updateData.UseStarted = false;
    }
    #endregion Interaction
}
