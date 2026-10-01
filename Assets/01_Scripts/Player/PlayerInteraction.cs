using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform rayPos;
    [SerializeField] private float interactDistance;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] private InteractionMessage messageUI;

    private PlayerBindInput input;
    private PlayerRest playerRest;

    private IInteractable currentTarget;

#if UNITY_EDITOR
    private void Reset()
    {
        interactDistance = 1.5f;
    }
#endif

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();
        playerRest = GetComponent<PlayerRest>();
        messageUI = GetComponentInChildren<InteractionMessage>();
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
            currentTarget = null;
            messageUI.HideText();
            return;
        }

        currentTarget = FindInteractable();

        if (currentTarget == null && NPCManager.Instance != null)
        {
            currentTarget = NPCManager.Instance.CurrentNPC;
        }

        if (currentTarget != null &&
            currentTarget.CanInteract(gameObject))
        {
            messageUI.ShowText(currentTarget.InteractionText, this.transform);
        }
        else
        {
            messageUI.HideText();
        }
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

