using UnityEngine;

[RequireComponent(typeof(PlayerBindInput))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform rayPos;
    [SerializeField] private float interactDistance;
    [SerializeField] private LayerMask interactableLayer;

    [SerializeField] private InteractionMessage message;

    private PlayerBindInput input;
    private PlayerRest playerRest;

    private IInteractable currentTarget;

#if UNITY_EDITOR
    private void Reset()
    {
        interactDistance = 2f;
    }
#endif

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();
        playerRest = GetComponent<PlayerRest>();

        rayPos = transform.FindChildByName("RayPos");
    }

    private void OnEnable()
    {
        input.OnInteractInput += TryInteract;
    }

    private void OnDisable()
    {
        input.OnInteractInput -= TryInteract;

        currentTarget = null;

        if (message != null)
            message.HideText();
    }

    private void Update()
    {
        if (message == null)
            return;

        // 쉬는 중에는 메세지 숨기기
        if (playerRest != null && playerRest.BRest)
        {
            currentTarget = null;
            message.HideText();
            return;
        }

        currentTarget = FindInteractable();

        // raycast 못찾았을 경우 현재 NPC 확인
        if (currentTarget == null && NPCManager.Instance != null)
        {
            currentTarget = NPCManager.Instance.CurrentNPC;
        }

        if (currentTarget == null || !currentTarget.CanInteract(gameObject))
        {
            message.HideText();
            return;
        }

        Component targetComponent = currentTarget as Component;

        if (targetComponent == null)
        {
            message.HideText();
            return;
        }

        message.ShowText(currentTarget.InteractionText, targetComponent.transform);

    }

    private void TryInteract()
    {
        // 쉬는 중이면 일어나기
        if (playerRest != null && playerRest.BRest)
        {
            playerRest.StandUp();
            return;
        }

        IInteractable target = FindInteractable();

        // raycast 못찾았을 경우 현재 NPC 확인
        if (target == null && NPCManager.Instance != null)
        {
            target = NPCManager.Instance.CurrentNPC;
        }

        if (target == null)
        {
            Debug.LogWarning("상호작용 할 대상이 없습니다.", this);
            return;
        }

        if (!target.CanInteract(gameObject))
        {
            return;
        }

        target.Interact(gameObject);
    }

    private IInteractable FindInteractable()
    {
        if (rayPos == null)
            return null;

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
