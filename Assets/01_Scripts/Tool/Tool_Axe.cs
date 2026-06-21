using UnityEngine;

public class Tool_Axe : ToolBase
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 충돌 방지
        if (other.transform.root == rootObject)
            return;

        if (!other.CompareTag("Tree"))
            return;

        equip.ReduceCurrentToolDurability();
    }
}
