using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ToolBase : MonoBehaviour
{
    protected Collider toolCollider;

    protected ItemData itemData;
    protected ToolData toolData;

    protected PlayerEquip equip;

    protected Transform rootObject;

    public ToolType ToolType => toolData.toolType;
    public int Rate => toolData.rate;

    protected virtual void Awake()
    {
        toolCollider = GetComponent<Collider>();
        toolCollider.isTrigger = true;
        toolCollider.enabled = false;
    }

    /// <summary>
    /// 도구 정보 설정
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="toolData"></param>
    /// <param name="equip"></param>
    /// <param name="root"></param>
    public virtual void Init(ItemData itemData, ToolData toolData, PlayerEquip equip, Transform root)
    {
        if (itemData == null)
        {
            Debug.LogError("itemData is null");
            return;
        }

        if (toolData == null)
        {
            Debug.LogError("toolData is null");
            return;
        }

        this.itemData = itemData;
        this.toolData = toolData;
        this.equip = equip;
        this.rootObject = root;
    }

    public void BeginUse()
    {
        toolCollider.enabled = true;
    }
    
    public void EndUse()
    {
        toolCollider.enabled = false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // 플레이어 충돌 방지
        if (other.transform.root == rootObject)
            return;
    }
}
