using UnityEngine;
using UnityEngine.AI;

public class NPCFollower : MonoBehaviour
{
    [Header("<< 따라가기 설정 >>")]
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float updateInterval = 0.2f;

    [Header("<< 애니메이션 설정 >>")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkParameterName = "IsWalking";

    private NavMeshAgent agent;
    private Transform player;
    private float updateTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            FollowPlayer();
        }

        UpdateAnimation();
    }

    private void FollowPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if (distance <= stopDistance)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isWalking = !agent.isStopped && agent.velocity.magnitude > 0.1f;

        animator.SetBool(walkParameterName, isWalking);
    }
}