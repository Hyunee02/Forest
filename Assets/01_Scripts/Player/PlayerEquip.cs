using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerEquip : MonoBehaviour
{
    [SerializeField] private Transform toolPos;

    private PlayerInventory inventory;
    private PlayerBindInput input;

    private GameObject currentToolObject;
    private ToolBase currentTool;

    private ItemData_SO currentItemData;
    private ToolData_SO currentToolData;
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
        input.OnAttackInput += UseTool;
    }

    private void OnDisable()
    {
        input.OnAttackInput -= UseTool;
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    public bool EquipTool(int slotIndex)
    {
        InventoryItem inventoryItem =
            inventory.GetItem(slotIndex);

        if (inventoryItem == null || inventoryItem.BEmpty)
            return false;

        ItemData_SO itemData =
            inventory.GetItemData(inventoryItem.itemId);

        ToolData_SO toolData =
            inventory.GetToolData(inventoryItem.itemId);

        if (itemData == null ||
            toolData == null ||
            itemData.prefab == null)
        {
            return false;
        }

        UnEquipTool();

        currentToolObject =
            Instantiate(
                itemData.prefab,
                toolPos,
                false
            );

        currentTool =
            currentToolObject
                .GetComponentInChildren<ToolBase>();

        if (currentTool == null)
        {
            Destroy(currentToolObject);
            return false;
        }

        currentInventoryItem = inventoryItem;
        currentItemData = itemData;
        currentToolData = toolData;

        currentTool.Init(
            itemData,
            toolData,
            this,
            transform
        );

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
        if (currentInventoryItem == null)
            return;

        if (currentToolData == null)
            return;

        bool usable =
            inventory.ReduceDurability(
                currentInventoryItem,
                currentToolData.reduce
            );

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
        currentItemData = null;
        currentToolData = null;
        currentInventoryItem = null;
    }
}