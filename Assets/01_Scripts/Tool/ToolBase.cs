using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ToolBase : MonoBehaviour, IEquipable
{
    protected new Collider collider;

    protected ToolData data;

    public int Id => data.itemId;
    public ToolType ToolType => data.toolType;
    public string Name => data.name;
    public int Rate => data.rate;
    public int Durability => data.durability;
    public int Reduce => data.reduce;

    public abstract void Init(ToolData data);

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
