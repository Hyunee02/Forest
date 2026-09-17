using UnityEngine;


public abstract class MapObject : MonoBehaviour
{
    [Header("<< Map Object >>")]
    [SerializeField] protected int width = 1;
    [SerializeField] protected int height = 1;
    [SerializeField] protected bool movable = true;

    [Header("Drop Item")]
    [SerializeField] protected WorldItem dropItemPrefab;
    [SerializeField] protected float minDropRadius;
    [SerializeField] protected float maxDropradius;
    [SerializeField] protected float dropHeight;

    [Header("Drop Animation")]
    [SerializeField] protected float jumpHeight;
    [SerializeField] protected float dropDuration;

    [Header("Hit")]
    [SerializeField, Min(1)] protected int maxHitCount;
    protected int hitCount;
    protected bool bDestoryed;

    public int Width => width;
    public int Height => height;
    public bool Movable => movable;

    /// <summary>
    /// 반경 이내 아이템 드롭
    /// </summary>
    protected virtual void DropItem()
    {
        if (dropItemPrefab == null)
            return;

        float angle = Random.Range(0f, Mathf.PI * 2f);

        Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

        // 최소 반경보다 가까이 떨어지지 않게
        float minRadius = Mathf.Max(0f, minDropRadius);
        float maxRadius = Mathf.Max(minRadius, maxDropradius);
        float distance = Random.Range(minRadius, maxRadius);

        // 출발 위치
        Vector3 startPos = transform.position + Vector3.up * dropHeight;
        
        // 도착 위치
        Vector3 endPos = startPos + direction * distance;

        WorldItem item = Instantiate(dropItemPrefab, startPos, Quaternion.identity);

        ItemDropMotion motion = item.GetComponent<ItemDropMotion>();

        motion.Play(endPos, jumpHeight, dropDuration);
    }

    /// <summary>
    /// 오브젝트 파괴
    /// </summary>
    protected virtual void DestroyObject()
    {
        if (bDestoryed)
            return;

        bDestoryed = true;
        Destroy(gameObject);
    }
}
