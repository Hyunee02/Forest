using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerEquip : MonoBehaviour
{
    [SerializeField] private Transform toolPos;

    private PlayerInventory inventory;
    private PlayerBindInput input;

    private GameObject currentToolObject;
    private ToolBase currentTool;
    private ToolData currentToolData;
    private InventoryItem currentInventoryItem;

#if UNITY_EDITOR
    private void Reset()
    {
        if (toolPos == null)
            toolPos = transform.FindChildByName("ToolPos");
    }
#endif

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        input = GetComponent<PlayerBindInput>();
    }

    private void OnEnable()
    {
        input.OnUseInput += UseTool;
    }

    private void OnDisable()
    {
        input.OnUseInput -= UseTool;
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool EquipTool(int slotIndex)
    {
        // 인벤토리 아이템 가져오기
        InventoryItem inventoryItem = inventory.GetItem(slotIndex);

        // 인벤토리 아이템 null 방지
        if (inventoryItem == null)
            return false;

        ItemData itemData = ItemLoadManager.Instance.GetItemData(inventoryItem.itemId);
        ToolData toolData = ItemLoadManager.Instance.GetToolData(inventoryItem.itemId);
        GameObject prefab = ItemLoadManager.Instance.GetItemPrefab(inventoryItem.itemId);

        // null 방지
        bool bNull = itemData == null
            || toolData == null
            || prefab == null;

        if (bNull)
            return false;

        // 장착 도구 해제
        UnEquipTool();

        // 도구 생성
        currentToolObject = Instantiate(prefab, toolPos, false);

        // ToolBase 안의 함수 실행하기 위해 가져옴
        currentTool = currentToolObject.GetComponentInChildren<ToolBase>();

        // 현재 생성 도구 null 방지
        if (currentTool == null)
        {
            Destroy(currentToolObject);
            return false;
        }

        currentInventoryItem = inventoryItem;
        currentToolData = toolData;

        // ToolBase 초기화
        currentTool.Init(itemData, toolData, this, transform);

        return true;
    }

    /// <summary>
    /// 도구 사용
    /// </summary>
    public void UseTool()
    {
        if (currentTool == null)
            return;

        currentTool.BeginUse();
    }

    /// <summary>
    /// 도구 사용 끝
    /// </summary>
    private void EndUseTool()
    {
        if (currentTool != null)
            currentTool.EndUse();
    }

    /// <summary>
    /// 도구 내구도 감소
    /// </summary>
    public void ReduceCurrentToolDurability()
    {
        // 현재 인벤토리 아이템 null 방지
        if (currentInventoryItem == null)
            return;

        if (currentToolData == null)
            return;

        // 사용 가능한지
        bool usable = inventory.ReduceDurability(currentInventoryItem, currentToolData.reduce);

        // 사용 불가능하면, 도구 장착 해제
        if (!usable)
            UnEquipTool();
    }

    /// <summary>
    /// 도구 장착 해제
    /// </summary>
    public void UnEquipTool()
    {
        if (currentToolObject != null)
            Destroy(currentToolObject);

        currentToolObject = null;
        currentTool = null;
        currentToolData = null;
        currentInventoryItem = null;
    }
}
