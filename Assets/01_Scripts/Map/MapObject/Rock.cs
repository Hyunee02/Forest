using UnityEngine;

public class Rock : MapObject, IHitTarget
{

#if UNITY_EDITOR

    public void Reset()
    {
        minDropRadius = 1.2f;
        maxDropradius = 2f;
        dropHeight = 0.15f;

        jumpHeight = 0.8f;
        dropDuration = 0.4f;
    }

#endif

    private void Start()
    {
        hitCount = 0;
        bDestoryed = false;
    }

    public bool CanHit(ToolType toolType)
    {
        return !bDestoryed
            && toolType == ToolType.Pickaxe;
    }

    public void Hit(int damage)
    {
        if (bDestoryed)
            return;

        hitCount++;

        DropItem();

        if (hitCount >= maxHitCount)
            base.DestroyObject();
    }
}
