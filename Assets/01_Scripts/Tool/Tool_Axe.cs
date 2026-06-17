using UnityEngine;

public class Tool_Axe : ToolBase
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 충돌 처리 방지
        if (other.gameObject == rootObject)
            return;

        if (!other.CompareTag("Tree"))
            return;

        ReduceDurability();
    }
}
