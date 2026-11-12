using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FishingFish : MonoBehaviour
{
    [Header("획득 아이템")]
    [SerializeField] private ItemData_SO itemData;

    [Header("찌 발견")]
    [SerializeField, Min(0.1f)] private float detectDistance = 2f;
    [SerializeField, Range(0f, 180f)] private float detectAngle = 100f;

    [Header("접근")]
    [SerializeField, Min(0.1f)] private float approachSpeed = 0.8f;
    [SerializeField, Min(1f)] private float rotateSpeed = 180f;
    [SerializeField, Min(0.01f)] private float stopDistance = 0.15f;

    [Header("도망")]
    [SerializeField, Min(0.1f)] private float escapeSpeed = 3f;
    [SerializeField, Min(0.1f)] private float escapeDuration = 0.5f;

    private PlayerFishing owner;

    private bool escaping;
    private float escapeEndsAt;
    private Vector3 escapeDirection;

    public ItemData_SO ItemData => itemData;

    public bool IsAvailable =>
        isActiveAndEnabled
        && !escaping
        && owner == null
        && itemData != null;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (!escaping)
            return;

        transform.position +=
            escapeDirection * escapeSpeed * Time.deltaTime;

        if (Time.time >= escapeEndsAt)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 물고기 앞쪽 가까운 위치의 찌만 발견
    /// </summary>
    public bool CanNotice(Vector3 bobberPosition)
    {
        if (!IsAvailable)
            return false;

        Vector3 direction = bobberPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > detectDistance * detectDistance)
            return false;

        if (direction.sqrMagnitude < 0.001f)
            return true;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        return Vector3.Angle(forward, direction)
            <= detectAngle * 0.5f;
    }

    /// <summary>
    /// 하나의 낚시에서만 이 물고기를 사용하도록 예약
    /// </summary>
    public bool TryReserve(PlayerFishing requester)
    {
        if (requester == null || !IsAvailable)
            return false;

        owner = requester;
        return true;
    }

    public bool IsOwnedBy(PlayerFishing requester)
    {
        return !escaping && owner == requester;
    }

    /// <summary>
    /// 찌 앞으로 접근. 도착하면 true
    /// </summary>
    public bool Approach(Vector3 bobberPosition)
    {
        Vector3 target = bobberPosition;
        target.y = transform.position.y;

        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;

        if (distance <= stopDistance)
            return true;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime);

        float moveDistance = Mathf.Min(
            approachSpeed * Time.deltaTime,
            distance - stopDistance);

        transform.position += direction.normalized * moveDistance;

        return distance - moveDistance <= stopDistance + 0.001f;
    }

    public void Release(PlayerFishing requester)
    {
        if (owner == requester)
            owner = null;
    }

    public void Escape(
        PlayerFishing requester,
        Vector3 bobberPosition)
    {
        if (owner != requester)
            return;

        owner = null;
        escaping = true;

        escapeDirection = transform.position - bobberPosition;
        escapeDirection.y = 0f;

        if (escapeDirection.sqrMagnitude < 0.001f)
        {
            escapeDirection = -transform.forward;
            escapeDirection.y = 0f;
        }

        if (escapeDirection.sqrMagnitude < 0.001f)
            escapeDirection = Vector3.forward;

        escapeDirection.Normalize();

        transform.rotation = Quaternion.LookRotation(escapeDirection);
        escapeEndsAt = Time.time + escapeDuration;
    }

    public void Caught(PlayerFishing requester)
    {
        if (owner != requester)
            return;

        owner = null;

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}