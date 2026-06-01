using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ToolBase : MonoBehaviour
{
    protected new Collider collider;

    protected ItemData itemData;
    protected ToolData toolData;

    public string Id => itemData.id;
    public string Name => itemData.name;
    public ToolType ToolType => toolData.toolType;
    public int Rate => toolData.rate;
    public int Durability => toolData.durability;
    public int Reduce => toolData.reduce;

    protected int curDurability;

    protected virtual void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public virtual void Init(ItemData itemData, ToolData toolData)
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

        curDurability = toolData.durability;
    }

    public virtual void Begin_Use()
    {

    }

    public virtual void End_Use()
    {

    }

    public virtual void Begin_Collision()
    {
        collider.enabled = true;
    }
    
    public virtual void End_Collision()
    {
        collider.enabled = false;
    }
}
