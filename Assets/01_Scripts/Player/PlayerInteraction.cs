using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform rayPos;

    [SerializeField] private float interactDistance = 1.5f;

    [SerializeField] private LayerMask interactableLayer;

    [SerializeField] private Message messageUI;

    private PlayerBindInput input;
    private PlayerRest playerRest;

    private IInteractable currentTarget;

#if UNITY_EDITOR
    private void Reset()
    {
        rayPos = gameObject.transform.FindChildByName("RayPos");
        messageUI = GetComponentInChildren<Message>();
    }
#endif

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();
        playerRest = GetComponent<PlayerRest>();
        messageUI = GetComponentInChildren<Message>();
    }

    private void OnEnable()
    {
        input.OnInteractInput += TryInteract;
    }

    private void OnDisable()
    {
        input.OnInteractInput -= TryInteract;
    }

    private void Update()
    {
        if (playerRest != null && playerRest.BRest)
        {
            messageUI.HideText();
            return;
        }

        currentTarget = FindInteractable();

        if (currentTarget != null && currentTarget.CanInteract(gameObject))
            messageUI.ShowText(currentTarget.InteractionText);

        else
            messageUI.HideText();

    }

    private void TryInteract()
    {
        if (playerRest != null && playerRest.BRest)
        {
            playerRest.StandUp();
            return;
        }

        IInteractable target = FindInteractable();

        if (target == null)
        {
            Debug.LogWarning("상호작용 할 대상이 없습니다.", this);
            return;
        }

        // 상호작용 가능한지
        if (!target.CanInteract(gameObject))
            return;

        target.Interact(gameObject);
    }

    private IInteractable FindInteractable()
    {
        Ray ray = new Ray(rayPos.position, rayPos.forward);

        if (Physics.Raycast(ray,
            out RaycastHit hit,
            interactDistance,
            interactableLayer))
            return hit.collider.GetComponentInParent<IInteractable>();

        return null;
    }

    private void OnDrawGizmos()
    {
        if (rayPos == null)
            return;

        Ray ray = new Ray(rayPos.position, rayPos.forward);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(rayPos.position, rayPos.position + rayPos.forward * interactDistance);

        if (Physics.Raycast(ray,
            out RaycastHit hit,
            interactDistance,
            interactableLayer))
        {
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
    }
}
