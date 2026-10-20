using UnityEngine;
using UnityEngine.Serialization;
using Util.SystemUtils;

public struct InteractionInitData
{
    public Camera Camera { get; set; }
    public CharacterController CharacterController { get; set; }

    public MenuPlayerCharacter MenuPlayerCharacter { get; set; }
    public FirstPersonPlayerCharacter FirstPersonPlayerCharacter { get; set; }
}

public struct InteractionUpdateData
{
    public float DeltaTime { get; set; }
}

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    InteractSettings m_InteractSettings;
    [SerializeField, FormerlySerializedAs("holdPos")]
    Transform m_HoldPos;
    [SerializeField]
    GameObject m_HUD;

    bool m_bInitialised = false;
    Camera m_Camera;
    CharacterController m_CharacterController;
    MenuPlayerCharacter m_MenuPC;
    FirstPersonPlayerCharacter m_FirstPersonPC;

    Interactable m_CurrentInteraction;

    public void Init(InteractionInitData initData)
    {
        m_bInitialised = true;
        m_Camera = initData.Camera;
        m_CharacterController = initData.CharacterController;
        m_MenuPC = initData.MenuPlayerCharacter;
        m_FirstPersonPC = initData.FirstPersonPlayerCharacter;
    }

    public void UpdateInteraction(ref InteractionUpdateData updateData)
    {
        Sys.Assert(m_bInitialised, "Player Interaction hasn't been initialised");
        if (m_CurrentInteraction == null) return;
        if (m_CurrentInteraction is Holdable heldObject)
            heldObject.LookAtPlayer(transform.position);
    }

    Interactable CheckForInteraction()
    {
        Interactable targetInteraction = null;
        if (Physics.SphereCast(
            origin: m_Camera.transform.position,
            radius: m_InteractSettings.InteractionRadius,
            direction: m_Camera.transform.forward,
            hitInfo: out RaycastHit hitInfo,
            maxDistance: m_InteractSettings.InteractionDistance,
            layerMask: m_InteractSettings.InteractableLayer,
            queryTriggerInteraction: QueryTriggerInteraction.Collide)
        ) targetInteraction = hitInfo.collider.GetComponent<Interactable>();
        return targetInteraction;
    }

    public void Interact()
    {
        Sys.Assert(m_bInitialised, "Player Interaction hasn't been initialised");
        Interactable targetInteraction = CheckForInteraction();
        if (targetInteraction != null)
        {
            if (targetInteraction is HouseDoor) targetInteraction.Interact(m_FirstPersonPC);
            else if (targetInteraction is Market)
            {
                InteractionInfo info = targetInteraction.Interact();
                if (info.OutArguments[0] is MarketMenu marketMenu)
                    m_MenuPC.OnMenuOpen(m_FirstPersonPC, m_HUD, marketMenu.gameObject);
            }
            else if (m_CurrentInteraction != null)
            {
                if (m_CurrentInteraction is WaterTank) targetInteraction.Interact(m_CurrentInteraction);
                else if (m_CurrentInteraction is Holdable) InteractHoldable();
            }
            else
            {
                m_CurrentInteraction = targetInteraction;
                if (m_CurrentInteraction is Holdable) InteractHoldable();
                else if (m_CurrentInteraction.Interact().EndInteraction) m_CurrentInteraction = null;
            }
        }
        else if (m_CurrentInteraction != null)
        {
            if (m_CurrentInteraction is Holdable) InteractHoldable();
        }
    }

    void InteractHoldable()
    {
        if (m_CurrentInteraction is Holdable holdable)
        {
            InteractionInfo info = holdable.Interact(
                m_HoldPos,
                m_InteractSettings.HoldLayer,
                m_CharacterController,
                m_Camera
            );
            if (info.EndInteraction) m_CurrentInteraction = null;
        }
    }

    public void Use()
    {
        Sys.Assert(m_bInitialised, "Player Interaction hasn't been initialised");
        if (m_CurrentInteraction == null) return;
        if (m_CurrentInteraction is Usable usable)
        {
            Interactable targetInteraction = CheckForInteraction();
            UseInfo info = usable.Use(targetInteraction);
            if (info.FinishedUsing)
            {
                Destroy(usable.gameObject);
                m_CurrentInteraction = null;
            }
        }
    }
}
