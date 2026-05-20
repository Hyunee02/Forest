using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ToolBase : MonoBehaviour
{
    protected new Collider collider;

    protected ToolData data;
    protected int id;

    public int Id => id;

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
