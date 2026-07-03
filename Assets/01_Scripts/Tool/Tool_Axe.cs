using UnityEngine;

public class Tool_Axe : ToolBase
{
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        // Tree가 아닐 시 충돌 X
        if (!other.CompareTag("Tree"))
            return;

        equip.ReduceCurrentToolDurability();
    }
}
