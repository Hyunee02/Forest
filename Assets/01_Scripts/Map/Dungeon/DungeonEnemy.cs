using System;
using UnityEngine;
using UnityEngine.AI;

public class DungeonEnemy : MonoBehaviour, IHitTarget
{
    [Header("<< HP >>")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp;

    [Header("<< 드랍 설정 >>")]
    [SerializeField] private string dropItemName = "던전 아이템";

    [Header("<< AI 설정 >>")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("<< 공격 설정 >>")]
    [SerializeField] private int attackDamage = 1;

    [Header("<< 컴포넌트 >>")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("<< 애니메이션 파라미터 >>")]
    [SerializeField] private string attackParameterName = "Attack";
    [SerializeField] private string getHitParameterName = "GetHit";
    [SerializeField] private string isDeadParameterName = "IsDead";

    [Header("<< Walk 애니메이션 사용 여부 >>")]
    [SerializeField] private bool useSpeedParameter = false;
    [SerializeField] private string speedParameterName = "Speed";

    private bool isDead;
    private float attackTimer;

    private Transform player;

    private Renderer[] renderers;
    private Collider[] colliders;

    public Action<DungeonEnemy> OnDied;

    private void Awake()
    {
        CacheComponents();
        FindPlayer();
        ResetEnemy();
    }

    private void Update()
    {
        if (isDead)
            return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        attackTimer -= Time.deltaTime;

        UpdateAI();

        if (useSpeedParameter)
        {
            UpdateMoveAnimation();
        }
    }

    private void CacheComponents()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    public void ResetEnemy()
    {
        CacheComponents();

        currentHp = maxHp;
        isDead = false;
        attackTimer = 0f;

        SetVisible(true);

        if (animator != null)
        {
            SetBoolIfExists(isDeadParameterName, false);

            if (useSpeedParameter)
                SetFloatIfExists(speedParameterName, 0f);
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    private void UpdateAI()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else if (distance <= detectRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMove();
        }
    }

    private void ChasePlayer()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void StopMove()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void AttackPlayer()
    {
        StopMove();
        LookAtPlayer();

        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        SetTriggerIfExists(attackParameterName);

        Debug.Log($"{gameObject.name}이(가) 플레이어를 공격했습니다. 데미지: {attackDamage}");

        // 플레이어 HP 코드 생기면 여기서 연결
        // player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }

    private void UpdateMoveAnimation()
    {
        if (animator == null)
            return;

        if (agent == null || !agent.isOnNavMesh)
        {
            SetFloatIfExists(speedParameterName, 0f);
            return;
        }

        float speed = agent.velocity.magnitude;
        SetFloatIfExists(speedParameterName, speed);
    }

    public void Hit(int damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"{gameObject.name} 공격됨. 남은 HP: {currentHp}");

        SetTriggerIfExists(getHitParameterName);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StopMove();

        SetBoolIfExists(isDeadParameterName, true);

        if (useSpeedParameter)
            SetFloatIfExists(speedParameterName, 0f);

        Debug.Log($"{gameObject.name} 처치됨. {dropItemName}을(를) 획득했습니다.");

        if (OnDied != null)
        {
            OnDied.Invoke(this);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}의 OnDied가 연결되지 않았습니다. 직접 비활성화합니다.");
            gameObject.SetActive(false);
        }
    }

    private void SetVisible(bool value)
    {
        if (renderers == null || colliders == null)
        {
            CacheComponents();
        }

        foreach (Renderer rend in renderers)
        {
            if (rend != null)
                rend.enabled = value;
        }

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = value;
        }
    }

    private bool HasParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (animator == null)
            return false;

        if (string.IsNullOrEmpty(parameterName))
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }

    private void SetTriggerIfExists(string parameterName)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(parameterName);
        }
    }

    private void SetBoolIfExists(string parameterName, bool value)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(parameterName, value);
        }
    }

    private void SetFloatIfExists(string parameterName, float value)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(parameterName, value);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}