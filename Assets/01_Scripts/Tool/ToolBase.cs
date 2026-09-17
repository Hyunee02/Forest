using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ToolBase : MonoBehaviour
{
    [Header("Tool Data")]
    [SerializeField] protected ToolData toolData;

    protected PlayerEquip equip;

    protected Collider toolCollider;
    protected Transform rootObject;

    private bool bUse;
    private bool bHit;

    public abstract ToolType ToolType { get; }

    public ToolData ToolData => toolData;
    public int Power => toolData.Power;
    public bool BUse => bUse;
    public int CurDurability
    {
        get
        {
            if (equip == null || equip.CurInventoryItem == null)
                return 0;

            return equip.CurInventoryItem.currentDurability;
        }
    }

    protected virtual void Awake()
    {
        toolCollider = GetComponent<Collider>();

        toolCollider.isTrigger = true;
        toolCollider.enabled = false;
    }

    public virtual void Init(PlayerEquip equip, Transform root, ToolData data)
    {
        this.equip = equip;
        rootObject = root;
        toolData = data;

        EndUse();
    }

    /// <summary>
    /// 도구 사용 여부 판단
    /// </summary>
    /// <returns></returns>
    public bool TryUse()
    {
        // null 방지
        if (equip == null || toolData == null)
            return false;

        // 애니메이션 중복 재생 방지
        if (bUse)
            return false;

        bUse = true;
        bHit = false;

        return true;
    }

    /// <summary>
    /// 애니메이션 타격 시작
    /// </summary>
    public void BeginCollision()
    {
        // 사용 중이면 false
        if (!bUse)
            return;

        toolCollider.enabled = true;
    }
    
    /// <summary>
    /// 애니메이션 타격 종료
    /// </summary>
    public void EndCollision()
    {
        toolCollider.enabled = false;
    }

    /// <summary>
    /// 전체 사용 애메이션 종료
    /// </summary>
    public void EndUse()
    {
        bUse = false;
        bHit = false;

        if (toolCollider != null)
            toolCollider.enabled = false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // 하나만 때릴 수 있게
        if (!bUse || bHit)
            return;

        // 플레이어 충돌 방지
        if (other.transform.root == rootObject)
            return;

        IHitTarget target = other.GetComponent<IHitTarget>();

        if (target == null)
            return;

        // 현재 도구로 때릴 수 있는 대상인지 검사
        if (!target.CanHit(ToolType))
            return;

        // 한 번 휘두를 때 한 번 작동
        bHit = true;

        target.Hit(Power);
        equip.ReduceEquippedDurability(toolData.DurabilityReduce);
    }

    ///// <summary>
    ///// 도구 내구도 감소
    ///// </summary>
    //private void ReduceDurability()
    //{
    //    int reduce = Mathf.Max(0, toolData.DurabilityReduce);

    //    curDurability = Mathf.Max(0, curDurability - reduce);

    //    Debug.Log($"{toolData.ToolName}의 내구도\n" +
    //        $"{curDurability}/{toolData.Durability}");

    //    if (curDurability <= 0)
    //        equip.UnEquipTool();
    //}
}
