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

    [Header("Test")]
    [SerializeField] private ToolData axeData;
    [SerializeField] private ToolData pickaxeData;

    private PlayerBindInput input;
    private PlayerInventory inventory;

    private GameObject curToolObject;
    private ToolBase curTool;
    private ToolData curToolData;
    private InventoryItem curInventoryItem;

    private bool bUse;
    private float nextUseTime;

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

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
            Test_EquipTool(axeData);

        if (Input.GetKey(KeyCode.Alpha2))
            Test_EquipTool(pickaxeData);
    }

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

    /// <summary>
    /// (테스트용) 도구 장착
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool Test_EquipTool(ToolData toolData)
    {
        bool bEquip = toolData == null
            || toolData.Prefab == null
            || handSocket == null;

        if (bEquip)
            return false;

        // 도구 해제
        UnEquipTool();

        // 도구 프리팹 생성 및 위치 조정
        curToolObject = Instantiate(toolData.Prefab, handSocket, false);
        //curTool.transform.localPosition = Vector3.zero;
        //curTool.transform.localRotation = Quaternion.identity;
        curTool = curToolObject.GetComponent<ToolBase>();

        // 현재 도구 null 방지
        if (curTool == null)
        {
            Destroy(curToolObject);
            ClearCurrentTool();
            return false;
        }

        curToolData = toolData;
        curTool.Init(this, rootObject, curToolData);

        if (animator != null)
            animator.SetInteger("ToolType", (int)curTool.ToolType);

        return true;
    }

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

        // 현재 아이템과 curInventoryItem이 같은 객체인지 확인
        if (ReferenceEquals(item, curInventoryItem))
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

        // 장착 상태 초기화
        ClearCurrentTool();


        curInventoryItem = item;
        curToolData = data;

        curToolObject = Instantiate(data.Prefab, handSocket, false);
        curTool = curToolObject.GetComponent<ToolBase>();
        animator.SetInteger("ToolType", (int)curTool.ToolType);

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
    }

    /// <summary>
    /// 도구 사용
    /// </summary>
    public void UseTool()
    {
        if (bUse)
            return;

        // 인벤토리 사용할 때 제한

        CheckEquippedItem();

        if (curTool == null || curToolData == null)
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

    public void ReduceEquippedDurability(int amount)
    {
        if (curInventoryItem == null)
            return;

        inventory.ReduceDurability(curInventoryItem, amount);
    }

    /// <summary>
    /// 장착 아이템 검사
    /// </summary>
    private void CheckEquippedItem()
    {
        if (curInventoryItem == null)
            return;

        if (!inventory.Contains(curInventoryItem) || curInventoryItem.currentDurability <= 0)
            ClearCurrentTool();
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
