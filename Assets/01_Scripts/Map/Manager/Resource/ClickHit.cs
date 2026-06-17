using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ClickHit : MonoBehaviour
{
    [Header("<< 공격 설정 >>")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private LayerMask targetLayer;

    [Header("<< 카메라 설정 >>")]
    [SerializeField] private Camera mainCamera;

    private Transform player;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryHit();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void TryHit()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera를 찾을 수 없습니다.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
        {
            IHitTarget target = hit.collider.GetComponentInParent<IHitTarget>();

            if (target == null)
                return;

            float distance = Vector3.Distance(player.position, hit.point);

            if (distance <= interactDistance)
            {
                int damage = GetCurrentDamage();
                target.Hit(damage);
            }
            else
            {
                Debug.Log("거리가 멀어서 공격할 수 없습니다.");
            }
        }
    }

    private int GetCurrentDamage()
    {
        // 나중에 무기 데이터 붙이면 여기서 무기 공격력 반환하면 됨.
        return defaultDamage;
    }

    private void OnDrawGizmosSelected()
    {
        Transform targetPlayer = player;

        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                targetPlayer = playerObj.transform;
        }

        if (targetPlayer == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPlayer.position, interactDistance);
    }
}