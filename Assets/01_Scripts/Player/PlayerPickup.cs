using UnityEngine;

[RequireComponent(typeof(PlayerBindInput), typeof(PlayerInventory))]
public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private float pickupRadius;

    private PlayerBindInput input;
    private PlayerInventory inventory;

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();
        inventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        input.OnInteractInput += TryPickup;
    }

    private void OnDisable()
    {
        input.OnInteractInput -= TryPickup;
    }

    /// <summary>
    /// 아이템 줍기
    /// </summary>
    private void TryPickup()
    {

        WorldItem target = FindNearestItem();

        if (target == null)
            return;

        // 인벤토리 저장
        target.Pickup(inventory);
    }


    private WorldItem FindNearestItem()
    {
        // IsTrigger 되어있어도 포함시켜서 탐색
        Collider[] colls = Physics.OverlapSphere(transform.position, pickupRadius, ~0, QueryTriggerInteraction.Collide);

        Debug.Log($"{pickupRadius}\n" +
            $"콜라이더 : {colls.Length}개");

        WorldItem nearestItem = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (Collider coll in colls)
        {
            // 플레이어 콜라이더 제외
            if (coll.transform.IsChildOf(transform))
                continue;

            WorldItem item = coll.GetComponent<WorldItem>();

            if (item == null
                || item.ItemData == null
                || item.Amount <= 0)
                continue;

            // 플레이어와 아이템 Collider 거리 비교
            Vector3 closestPoint = coll.ClosestPoint(transform.position);

            // 거리 계산 (루트 계산 생략하기 위해 sqrMagnitude)
            float distance = (closestPoint - transform.position).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestItem = item;
            }
        }

        return nearestItem;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
