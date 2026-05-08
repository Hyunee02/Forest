using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Interactable : MonoBehaviour
{
    [Header("<< 상호작용 >>")]
    [SerializeField] protected float interactionRange = 1f;
    [SerializeField] protected bool isPlayerInRange;

    protected Transform player;

    public bool IsPlayerInRange => isPlayerInRange;

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        CheckPlayerDistance();

        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    protected virtual void CheckPlayerDistance()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool isInCurrentRange = distance <= interactionRange;

        if (isInCurrentRange == isPlayerInRange)
            return;

        isPlayerInRange = isInCurrentRange;

        if (isPlayerInRange)
        {
            OnEnterInteractionRange();
        }
        else
        {
            OnExitInteractionRange();
        }
    }

    protected abstract void Interact();

    protected virtual void OnEnterInteractionRange()
    {
        Debug.Log($"{gameObject.name} 상호작용 가능");
    }

    protected virtual void OnExitInteractionRange()
    {
        Debug.Log($"{gameObject.name} 상호작용 범위 벗어남");
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = isPlayerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}