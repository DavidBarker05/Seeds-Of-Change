using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    Camera cam;
    [SerializeField, Min(0f)]
    float maxInteractionDistance = 2f;
    [SerializeField, Range(0.1f, 0.5f)]
    float interactionRadius;
    [SerializeField]
    LayerMask interactableLayer;
    [SerializeField]
    Transform holdPos;
    [SerializeField]
    LayerMask holdLayer;
    [SerializeField]
    Camera holdCamera;
    [SerializeField]
    Camera holdClipCamera;

    Interactable currentInteraction;

    void Start()
    {
        InputManagerScript.Instance?.AddInteractAction(Interact);
        InputManagerScript.Instance?.AddUseAction(Use);
    }

    void Update()
    {
        if (GameManager.Instance?.IsPaused ?? false) return;
        if (currentInteraction == null) return;
        if (currentInteraction is Holdable heldObject)
        {
            heldObject.LookAtPlayer(transform.position);
            int bitMask = ~(holdLayer | gameObject.layer);
            if (Physics.Linecast(cam.transform.position, heldObject.transform.position, bitMask) || Physics.CheckBox(heldObject.transform.position, heldObject.GetComponent<Collider>().bounds.extents, heldObject.transform.rotation, bitMask))
            {
                holdCamera.gameObject.SetActive(true);
                holdClipCamera.gameObject.SetActive(true);
            }
            else
            {
                holdCamera.gameObject.SetActive(false);
                holdClipCamera.gameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        InputManagerScript.Instance?.RemoveInteractAction(Interact);
        InputManagerScript.Instance?.RemoveUseAction(Use);
    }

    Interactable CheckForInteraction()
    {
        Interactable targetInteraction = null;
        if (Physics.SphereCast(cam.transform.position, interactionRadius, cam.transform.forward, out RaycastHit interactHit, maxInteractionDistance, interactableLayer, QueryTriggerInteraction.Collide)) targetInteraction = interactHit.collider.GetComponent<Interactable>();
        return targetInteraction;
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        Interactable targetInteraction = CheckForInteraction();
        if (targetInteraction != null)
        {
            if (targetInteraction is HouseDoor) targetInteraction.Interact(gameObject);
            else if (targetInteraction is Market) targetInteraction.Interact();
            else if (currentInteraction != null)
            {
                if (targetInteraction is WaterTank) targetInteraction.Interact(currentInteraction);
                else if (currentInteraction is Holdable) InteractHoldable();
            }
            else
            {
                currentInteraction = targetInteraction;
                if (currentInteraction is Holdable) InteractHoldable();
                else if (currentInteraction.Interact()) currentInteraction = null;
            }
        }
        else if (currentInteraction != null)
        {
            if (currentInteraction is Holdable) InteractHoldable();
        }
    }

    void InteractHoldable()
    {
        if (currentInteraction is Holdable holdable)
        {
            bool drop = holdable.Interact(holdPos, holdLayer, GetComponent<Collider>(), cam);
            if (drop) currentInteraction = null;
        }
    }

    void Use(InputAction.CallbackContext ctx)
    {
        if (currentInteraction == null) return;
        if (currentInteraction is Usable usable)
        {
            Interactable targetInteraction = CheckForInteraction();
            bool finishedUsing = usable.Use(targetInteraction);
            if (finishedUsing) Destroy(usable.gameObject);
        }
    }
}
