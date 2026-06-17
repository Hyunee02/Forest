using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ToolBase : MonoBehaviour
{
    protected Collider toolCollider;

    protected ItemData itemData;
    protected ToolData toolData;

    protected Transform rootObject;

    protected int curDurability;

    public string Id => itemData.id;
    public string Name => itemData.name;
    public ToolType ToolType => toolData.toolType;
    public int Rate => toolData.rate;
    public int Durability => toolData.durability;
    public int Reduce => toolData.reduce;

    protected virtual void Awake()
    {
        toolCollider = GetComponent<Collider>();
        toolCollider.isTrigger = true;
        toolCollider.enabled = false;
    }

    public virtual void Init(ItemData itemData, ToolData toolData, Transform root)
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
        this.rootObject = root;

        curDurability = toolData.durability;
        End_Collision();
    }

    protected void ReduceDurability()
    {
        curDurability -= Reduce;

        if (curDurability <= 0)
            Destroy(gameObject);
    }

    public void Begin_Collision()
    {
        toolCollider.enabled = true;
    }
    
    public void End_Collision()
    {
        toolCollider.enabled = false;
    }
}
