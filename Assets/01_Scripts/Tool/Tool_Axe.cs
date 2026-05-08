using UnityEngine;

public class Tool_Axe : ToolBase
{
    private ToolData data;

    private GameObject rootObject;

    private string toolPosName = "ToolPos";
    private Transform toolPos;

    [Header("----- Info -----")]
    [SerializeField] private int toolId;
    [SerializeField] private int curDurability;

    private void Awake()
    {
        collider = GetComponent<Collider>();

        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null, "RootObject is null");

        toolPos = rootObject.transform.FindChildByName(toolPosName);
        Debug.Assert(toolPosName != null, "ToolPosName is null");

        transform.SetParent(toolPos, false);
    }

    // ToolData 적용
    public override void Init(ToolData data)
    {
        if (data == null)
        {
            Debug.LogError("ToolData is null");
            return;
        }

        this.data = data;
        Debug.Log($"Apply Completely\nID : {data.id}\nName : {data.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 충돌 처리 방지
        if (other.gameObject == rootObject)
            return;

        // 나무에만 충돌 가능
        if (other.CompareTag("Tree"))
        {
            curDurability -= data.reduce;

            if (curDurability < 0)
                Destroy(gameObject);
        }
    }
}
