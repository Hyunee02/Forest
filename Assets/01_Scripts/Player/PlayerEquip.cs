using System;
using UnityEngine;

[RequireComponent(typeof(PlayerBindInput))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerEquip : MonoBehaviour
{
    [Header("Equip")]
    [SerializeField] private Transform handSocket;
    [SerializeField] private Transform rootObject;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string equipTriggerName;

    [Header("UI")]
    [SerializeField] private InventoryUI inventoryUI;

    private PlayerBindInput input;
    private PlayerInventory inventory;

    private GameObject curToolObject;
    private ToolBase curTool;
    private ToolData curToolData;
    private InventoryItem curInventoryItem;

    private bool bUse;
    private float nextUseTime;

    public event Action OnEquipChanged;

    public ToolBase CurTool => curTool;
    public ToolData CurToolData => curToolData;
    public InventoryItem CurInventoryItem => curInventoryItem;

    public bool BTool => curTool != null;
    public bool BUse => bUse;

#if UNITY_EDITOR

    private void Reset()
    {
        equipTriggerName = "UseTool";
    }

#endif

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();
        inventory = GetComponent<PlayerInventory>();

        if (rootObject == null)
            rootObject = transform.root;

        if (handSocket == null)
            handSocket = Helper.FindChildByName(this.transform, "HandSocket");

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        input.OnUseInput += UseTool;
        inventory.OnChanged += CheckEquippedItem;

        CheckEquippedItem();
    }

    private void OnDisable()
    {
        input.OnUseInput -= UseTool;
        inventory.OnChanged -= CheckEquippedItem;

        EndUseTool();
    }

    public void BindInventoryUI(InventoryUI ui)
    {
        inventoryUI = ui;
    }

    /// <summary>
    /// item 아이템 장착중인지 확인
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool IsEquipped(InventoryItem item)
    {
        return item != null
            && !item.BEmpty
            && ReferenceEquals(item, curInventoryItem);
    }

    /// <summary>
    /// 장착 아이템 검사
    /// </summary>
    private void CheckEquippedItem()
    {
        if (curInventoryItem == null)
            return;

        if (!inventory.Contains(curInventoryItem) || curInventoryItem.currentDurability <= 0)
        {
            ClearCurrentTool();

            OnEquipChanged?.Invoke();
        }
    }

    /// <summary>
    /// 아이템 장착하기 버튼
    /// </summary>
    /// <param name="slotIndex"></param>
    public void ToggleEquip(int slotIndex)
    {
        if (bUse)
            return;

        InventoryItem item = inventory.GetItem(slotIndex);

        if (item == null || item.BEmpty)
            return;

        if (ReferenceEquals(item, curInventoryItem))
            UnEquipTool();

        else
            EquipTool(slotIndex);
    }

    #region > 도구 장착 및 사용
    /// <summary>
    /// 도구 장착
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool EquipTool(int slotIndex)
    {
        if (bUse)
            return false;

        InventoryItem item = inventory.GetItem(slotIndex);

        if (item == null || item.BEmpty)
            return false;

        if (IsEquipped(item))
            return true;

        ItemData_SO itemData = inventory.GetItemData(item.itemId);

        // 장착 가능한 도구인지 검사
        if (itemData == null
            || itemData.itemType != ItemTypeSO.Tool)
            return false;

        ToolData data = inventory.GetToolData(item.itemId);

        bool bEquip = data == null
            || data.Prefab == null
            || handSocket == null
            || item.currentDurability <= 0;

        if (bEquip)
            return false;

        if (data.Prefab.GetComponent<ToolBase>() == null)
            return false;

        // 장착 상태 초기화
        ClearCurrentTool();


        curInventoryItem = item;
        curToolData = data;

        curToolObject = Instantiate(data.Prefab, handSocket, false);
        curTool = curToolObject.GetComponent<ToolBase>();
        curTool.Init(this, rootObject, curToolData);
        animator.SetInteger("ToolType", (int)curTool.ToolType);

        OnEquipChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// 도구 장착 해제
    /// </summary>
    public void UnEquipTool()
    {
        if (bUse)
            return;

        // 현재 도구 정보 초기화
        ClearCurrentTool();

        OnEquipChanged?.Invoke();
    }

    /// <summary>
    /// 도구 사용
    /// </summary>
    public void UseTool()
    {
        if (bUse)
            return;

        if (inventoryUI != null && inventoryUI.BOpen)
            return;

        CheckEquippedItem();

        bool bNull = curTool == null
            || curToolData == null
            || curInventoryItem == null
            || animator == null;

        if (bNull)
            return;

        if (curInventoryItem.currentDurability <= 0)
            return;

        if (Time.time < nextUseTime)
            return;

        // 사용 가능 여부 검사
        if (!curTool.TryUse())
            return;

        bUse = true;

        nextUseTime = Time.time + Mathf.Max(0f, curToolData.Cooldown);

        animator.SetInteger("ToolType", (int)curTool.ToolType);
        animator.SetTrigger(equipTriggerName);
    }

    /// <summary>
    /// 도구 사용 끝
    /// </summary>
    public void EndUseTool()
    {
        if (curTool != null)
            curTool.EndUse();

        bUse = false;

        if (curTool == null)
            animator.SetInteger("ToolType", 0);
    }
    #endregion

    /// <summary>
    /// 장착 도구 내구도 감소
    /// </summary>
    /// <param name="amount"></param>
    public void ReduceEquippedDurability(int amount)
    {
        if (curInventoryItem == null)
            return;

        inventory.ReduceDurability(curInventoryItem, amount);
    }

    /// <summary>
    /// 현재 아이템 초기화
    /// </summary>
    private void ClearCurrentTool()
    {
        if (curTool != null)
            curTool.EndUse();

        if (curToolObject != null)
        {
            curToolObject.SetActive(false);
            Destroy(curToolObject);
        }

        curToolObject = null;
        curTool = null;
        curToolData = null;
        curInventoryItem = null;

        // 사용 중 파손되면 0으로 변경
        if (!bUse)
            animator.SetInteger("ToolType", 0);
    }
}
