using UnityEngine;

public class Tool_Axe : ToolBase
{
    private GameObject rootObject;

    //private string toolPosName = "ToolPos";
    //private Transform toolPos;

    protected override void Awake()
    {
        base.Awake();

        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null, "RootObject is null");

        //toolPos = rootObject.transform.FindChildByName(toolPosName);
        //Debug.Assert(toolPosName != null, "ToolPosName is null");

        //transform.SetParent(toolPos, false);
    }

    // ToolData 적용
    public override void Init(ItemData itemData, ToolData toolData)
    {
        base.Init(itemData, toolData);
        Debug.Log($"Apply Completely\nID : {itemData.id}\nName : {itemData.name}");

        collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 충돌 처리 방지
        if (other.gameObject == rootObject)
            return;

        // 나무에만 충돌 가능
        if (other.CompareTag("Tree"))
        {
            curDurability -= toolData.reduce;

            if (curDurability < 0)
                Destroy(gameObject);
        }
    }

    public override void Begin_Use()
    {
        
    }

    public override void End_Use()
    {
        
    }
}
