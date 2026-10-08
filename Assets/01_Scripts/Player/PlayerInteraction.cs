using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform rayPos;
    [SerializeField] private float interactDistance;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI")]
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

        if (playerRest != null && playerRest.BRest)
        {
            currentTarget = null;
            message.HideText();
            return;
        }

        // Raycast로 상호작용 대상 찾기
        currentTarget = FindInteractable();

        // Raycast로 찾은 대상 없으면 범위 내의 NPC 가져오기
        if (currentTarget == null && NPCManager.Instance != null)
        {
            currentTarget = NPCManager.Instance.CurrentNPC;
        }

        // 대상 없거나 상호작용 할 수 없으면 메시지 숨기기
        if (currentTarget == null || !currentTarget.CanInteract(gameObject))
        {
            message.HideText();
            return;
        }

        // 대상의 Transform 가져오기 위해 컴포넌트로 변환
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
        // 앉아있는 상태면 먼저 일어나기
        if (playerRest != null && playerRest.BRest)
        {
            playerRest.StandUp();
            return;
        }

        IInteractable target = FindInteractable();

        if (target == null && NPCManager.Instance != null)
        {
            target = NPCManager.Instance.CurrentNPC;
        }

        if (target == null)
        {
            Debug.LogWarning("npc가 없습니다.", this);
            return;
        }

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
    }
}

