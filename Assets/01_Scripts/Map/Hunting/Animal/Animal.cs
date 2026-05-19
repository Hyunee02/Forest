using System.Collections;
using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("<< HP >>")]
    [SerializeField] protected int maxHp = 100;
    [SerializeField] protected int currentHp;
    [SerializeField] protected float respawnDelay = 4f;

    [Header("<< 애니메이션 >>")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected string walkParameterName = "IsWalking";

    [Header("<< Idle >>")]
    [SerializeField] protected float minIdleTime = 2f;
    [SerializeField] protected float maxIdleTime = 5f;

    [Header("<< Walk >>")]
    [SerializeField] protected float moveSpeed = 0.7f;
    [SerializeField] protected float rotateSpeed = 0.5f;
    [SerializeField] protected float minWalkTime = 0.8f;
    [SerializeField] protected float maxWalkTime = 1.5f;

    [Header("<< 범위 >>")]
    [SerializeField] protected float maxRoamDistance = 5f;

    protected Vector3 spawnPosition;
    protected Vector3 moveDirection;

    protected float stateTimer;
    protected bool isWalking;
    protected bool isDead;

    protected Renderer[] renderers;
    protected Collider[] colliders;


    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        spawnPosition = transform.position;

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        currentHp = maxHp;
    }

    protected virtual void Start()
    {
        StartIdle();
    }

    protected virtual void Update()
    {
        if (isDead)
            return;

        stateTimer -= Time.deltaTime;

        if (isWalking)
        {
            Walk();

            if (stateTimer <= 0f)
            {
                StartIdle();
            }
        }
        else
        {
            if (stateTimer <= 0f)
            {
                StartWalk();
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isWalking = false;

        if (animator != null)
            animator.SetBool(walkParameterName, false);

        Vector3 respawnPosition = spawnPosition;

        AnimalPoolManager.Instance.RespawnAnimal(this, respawnDelay, respawnPosition);
    }

    protected virtual IEnumerator RespawnRoutine()
    {
        Vector3 deathPosition = transform.position;

        SetVisible(false);

        yield return new WaitForSeconds(respawnDelay);

        transform.position = deathPosition;
        currentHp = maxHp;
        isDead = false;

        SetVisible(true);
        StartIdle();
    }

    public virtual void ResetAnimal(Vector3 respawnPosition)
    {
        transform.position = respawnPosition;

        currentHp = maxHp;
        isDead = false;
        isWalking = false;

        spawnPosition = respawnPosition;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool(walkParameterName, false);
        }

        StartIdle();
    }

    protected virtual void SetVisible(bool value)
    {
        foreach (Renderer rend in renderers)
        {
            rend.enabled = value;
        }

        foreach (Collider col in colliders)
        {
            col.enabled = value;
        }
    }

    protected virtual void StartIdle()
    {
        isWalking = false;
        stateTimer = Random.Range(minIdleTime, maxIdleTime);

        if (animator != null)
            animator.SetBool(walkParameterName, false);
    }

    protected virtual void StartWalk()
    {
        isWalking = true;
        stateTimer = Random.Range(minWalkTime, maxWalkTime);

        moveDirection = GetMoveDirection();

        if (animator != null)
            animator.SetBool(walkParameterName, true);
    }

    protected virtual void Walk()
    {
        Vector3 flatDirection = moveDirection;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(flatDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );

        Vector3 nextPosition = transform.position + transform.forward * moveSpeed * Time.deltaTime;

        float distanceFromSpawn = Vector3.Distance(spawnPosition, nextPosition);

        if (distanceFromSpawn <= maxRoamDistance)
        {
            transform.position = nextPosition;
        }
        else
        {
            moveDirection = (spawnPosition - transform.position).normalized;
        }
    }

    protected virtual Vector3 GetMoveDirection()
    {
        float distanceFromSpawn = Vector3.Distance(spawnPosition, transform.position);

        if (distanceFromSpawn > maxRoamDistance * 0.7f)
        {
            return (spawnPosition - transform.position).normalized;
        }

        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        return randomDirection.normalized;
    }


    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        Gizmos.DrawWireSphere(center, maxRoamDistance);
    }
}
