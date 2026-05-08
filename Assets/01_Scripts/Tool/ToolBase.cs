using UnityEngine;

public abstract class ToolBase : MonoBehaviour
{
    protected new Collider collider;

    public abstract void Init(ToolData data);

    public virtual void Begin_Collision()
    {
        collider.enabled = true;
    }
    
    public virtual void End_Collision()
    {
        collider.enabled = false;
    }
}
