//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class PlayerInventory : MonoBehaviour
//{
//    [SerializeField] private Transform dropPoint;

//    [Header("----- UI -----")]
//    [SerializeField] private RectTransform inventoryPanel;
//    [SerializeField] private GameObject optionPanel;
//    [SerializeField] private Transform slotParent;
//    [SerializeField] private Image dragIcon;

//    [Header("----- Option -----")]
//    [SerializeField] private Button equipButton;
//    [SerializeField] private Button dropButton;

//    private List<InventorySlot> slots = new List<InventorySlot>();
//    private InventoryItem[] items; 

//    private PlayerBindInput input;
//    private PlayerEquip equip;

//    private InventorySlot dragStartSlot;
//    private bool droppedOnSlot;

//    public bool BOpen { get { return inventoryPanel.gameObject.activeSelf; } }

//    private int selectedIndex = -1;
//    private Vector2 selectedMousePosition;

//    private void Awake()
//    {
//        input = GetComponent<PlayerBindInput>();
//        equip = GetComponent<PlayerEquip>();

//        InitSlots();

//        items = new InventoryItem[slots.Count];

//        inventoryPanel.gameObject.SetActive(false);
//        optionPanel.gameObject.SetActive(false);
//        dragIcon.gameObject.SetActive(false);
        
//        dragIcon.raycastTarget = false;

//        equipButton.onClick.AddListener(EquipSelectedItem);
//        dropButton.onClick.AddListener(DropSelectedItem);
//    }

//    private void OnEnable()
//    {
//        input.OnInventoryInput += ToggleInventory;
//        input.OnUseInput += UseSelectedSlot;
//    }

//    private void OnDisable()
//    {
//        input.OnInventoryInput -= ToggleInventory;
//        input.OnUseInput -= UseSelectedSlot;
//    }

//    private void Update()
//    {
//        if (Input.GetKey(KeyCode.Alpha1))
//            AddItem("0001", 1);
//    }

//    /// <summary>
//    /// 슬롯 초기화
//    /// </summary>
//    private void InitSlots()
//    {
//        // 슬롯 초기화
//        slots.Clear();

//        // 비활성화 슬롯들까지 포함해서 찾기
//        InventorySlot[] findSlots = slotParent.GetComponentsInChildren<InventorySlot>(true);

//        // 슬롯 초기화
//        for (int i = 0; i < findSlots.Length; i++)
//        {
//            findSlots[i].Init(this, i);
//            slots.Add(findSlots[i]);
//        }
//    }

//    /// <summary>
//    /// 인벤토리 아이템 가져오기
//    /// </summary>
//    /// <param name="index"></param>
//    /// <returns></returns>
//    public InventoryItem GetItem(int index)
//    {
//        if (index < 0 || index >= items.Length)
//            return null;

//        return items[index];
//    }

//    /// <summary>
//    /// 인벤토리 아이템 슬롯 번호 찾기
//    /// </summary>
//    /// <param name="targetItem"></param>
//    /// <returns></returns>
//    private int GetItemIndex(InventoryItem targetItem)
//    {
//        for (int i = 0; i < items.Length; i++)
//        {
//            if (items[i] == targetItem)
//                return i;
//        }

//        return -1;
//    }

//    /// <summary>
//    /// 아이템 추가하기
//    /// </summary>
//    /// <param name="itemId"></param>
//    /// <param name="amount"></param>
//    /// <returns></returns>
//    public bool AddItem(string itemId, int amount)
//    {
//        ItemData itemData = ItemLoadManager.Instance.GetItemData(itemId);
//        ToolData toolData = ItemLoadManager.Instance.GetToolData(itemId);

//        if (itemData == null || amount <= 0)
//            return false;

//        // 도구이면 1개, 아이템이면 maxStack
//        int maxStack = toolData != null ? 1 : Mathf.Max(1, itemData.maxStack);

//        // 아이템일 때, 기존 스택에 먼저 추가
//        if (maxStack > 1)
//        {

//            for (int i = 0; i < items.Length; i++)
//            {
//                bool bItem = items[i] != null
//                    && items[i].itemId == itemId
//                    && items[i].count < maxStack;

//                if (bItem)
//                {
//                    items[i].count++;
//                    RefreshSlot(i);
//                    return true;
//                }
//            }
//        }

//        // 빈 슬롯에 새로 추가
//        //for (int i = 0; i < items.Length; i++)
//        //{
//        //    if (items[i] == null)
//        //    {
//        //        //int durability = toolData != null ? toolData.durability : 0;

//        //        items[i] = new InventoryItem(itemId, 1, durability);

//        //        RefreshSlot(i);

//        //        return true;
//        //    }
//        //}

//        Debug.Log("인벤토리가 가득 찼습니다.");
//        return false;
//    }

//    /// <summary>
//    /// 드래그 및 아이템 합치기
//    /// </summary>
//    /// <param name="startIndex"></param>
//    /// <param name="dropIndex"></param>
//    private void MoveOrMerge(int startIndex, int dropIndex)
//    {
//        // 같은 슬롯이면 리턴
//        if (startIndex == dropIndex)
//            return;

//        if (items[startIndex] == null)
//            return;

//        bool sameItem = items[dropIndex] != null &&
//            items[dropIndex].itemId == items[startIndex].itemId;

//        // 같은 아이템이고 maxStack 안 넘으면 합치기
//        if (sameItem)
//        {
//            ItemData itemData = ItemLoadManager.Instance.GetItemData(items[startIndex].itemId);
//            ToolData toolData = ItemLoadManager.Instance.GetToolData(items[startIndex].itemId);

//            int maxStack = toolData != null ? 1 : Mathf.Max(1, itemData.maxStack);

//            if (maxStack > 1 && items[dropIndex].count < maxStack)
//            {
//                items[dropIndex].count++;
//                items[startIndex].count--;

//                if (items[startIndex].count <= 0)
//                    items[startIndex] = null;

//                RefreshSlot(startIndex);
//                RefreshSlot(dropIndex);

//                return;
//            }
//        }

//        // 자리 스왑
//        InventoryItem temp = items[startIndex];
//        items[startIndex] = items[dropIndex];
//        items[dropIndex] = temp;

//        // UI 갱신
//        RefreshSlot(startIndex);
//        RefreshSlot(dropIndex);
//    }

//    /// <summary>
//    /// 내구도 줄이기
//    /// </summary>
//    /// <param name="targetItem"></param>
//    /// <param name="amount"></param>
//    /// <returns></returns>
//    public bool ReduceDurability(InventoryItem targetItem, int amount)
//    {
//        if (targetItem == null)
//            return false;

//        int index = GetItemIndex(targetItem);

//        if (index < 0)
//            return false;

//        targetItem.currentDurability -= amount;

//        if (targetItem.currentDurability <= 0)
//        {
//            items[index] = null;
//            RefreshSlot(index);
//            return false;
//        }

//        RefreshSlot(index);

//        return true;
//    }

//    #region 인벤토리 슬롯 이동
//    /// <summary>
//    /// 드래그 시작
//    /// </summary>
//    /// <param name="slot"></param>
//    /// <param name="eventData"></param>
//    public void BeginDrag(InventorySlot slot, PointerEventData eventData)
//    {
//        if (items[slot.Index] == null)
//            return;

//        CloseOptionMenu();

//        // 시작 슬롯
//        dragStartSlot = slot;

//        // 드래그가 슬롯 위에 드롭됐는지
//        droppedOnSlot = false;

//        // 드래그 아이콘 표시
//        dragIcon.sprite = slot.GetIcon();
//        dragIcon.gameObject.SetActive(true);
//        dragIcon.transform.position = eventData.position;
//    }

//    /// <summary>
//    /// 드래그 중
//    /// </summary>
//    /// <param name="evenData"></param>
//    public void Drag(PointerEventData evenData)
//    {
//        if (dragStartSlot == null)
//            return;

//        dragIcon.transform.position = evenData.position;
//    }

//    /// <summary>
//    /// 드롭
//    /// </summary>
//    /// <param name="dropSlot"></param>
//    public void Drop(InventorySlot dropSlot)
//    {
//        if (dragStartSlot == null)
//            return;

//        droppedOnSlot = true;

//        MoveOrMerge(dragStartSlot.Index, dropSlot.Index);
//    }

//    /// <summary>
//    /// 드래그 종료
//    /// </summary>
//    public void EndDrag(PointerEventData eventData)
//    {
//        if (dragStartSlot == null)
//            return;

//        bool insideInventory = RectTransformUtility.RectangleContainsScreenPoint
//            (inventoryPanel, eventData.position, eventData.pressEventCamera);

//        // 인벤토리 안에 아이템이 드래그 되지 않았을 때, 아이템 제거 및 드롭
//        if (droppedOnSlot == false && insideInventory == false)
//            DropItem(dragStartSlot.Index);

//        dragStartSlot = null;
//        droppedOnSlot = false;

//        dragIcon.sprite = null;
//        dragIcon.gameObject.SetActive(false);
//    }
//    #endregion

//    #region 패널 온/오프
//    // 인벤토리 패널 온 / 오프
//    private void ToggleInventory()
//    {
//        inventoryPanel.gameObject.SetActive(!BOpen);

//        if (BOpen)
//        {
//            CloseOptionMenu();
//            dragIcon.gameObject.SetActive(false);
//        }
//    }

//    /// <summary>
//    /// 옵션 패널 켜기
//    /// </summary>
//    private void OpenOptionMenu()
//    {
//        ToolData toolData = ItemLoadManager.Instance.GetToolData(items[selectedIndex].itemId);

//        optionPanel.gameObject.SetActive(true);
//        equipButton.gameObject.SetActive(toolData != null);
//    }

//    /// <summary>
//    /// 옵션 패널 끄기
//    /// </summary>
//    private void CloseOptionMenu()
//    {
//        selectedIndex = -1;
//        optionPanel.gameObject.SetActive(false);
//    }
//    #endregion

//    /// <summary>
//    /// 슬롯 갱신
//    /// </summary>
//    /// <param name="index"></param>
//    private void RefreshSlot(int index)
//    {
//        InventoryItem item = items[index];

//        // 아이템 비어있으면 슬롯 비우기
//        if (item == null)
//        {
//            slots[index].Clear();
//            return;
//        }

//        // 아이콘, 데이터 가져오기
//        Sprite sprite = ItemLoadManager.Instance.GetItemSprite(item.itemId);
//        ToolData toolData = ItemLoadManager.Instance.GetToolData(item.itemId);

//        // 슬롯 UI 갱신
//        slots[index].SetItem(sprite, item.count, toolData, item.currentDurability);
//    }

//    /// <summary>
//    /// 슬롯 선택
//    /// </summary>
//    /// <param name="slot"></param>
//    /// <param name="mousePosition"></param>
//    public void SelectSlot(InventorySlot slot, Vector2 mousePosition)
//    {
//        if (items[slot.Index] == null)
//            return;

//        selectedIndex = slot.Index;
//        selectedMousePosition = mousePosition;
//    }

//    /// <summary>
//    /// 선택된 슬롯 아이템 사용
//    /// </summary>
//    private void UseSelectedSlot()
//    {
//        if (inventoryPanel.gameObject.activeSelf == false)
//            return;

//        if (selectedIndex < 0)
//            return;

//        if (items[selectedIndex] == null)
//            return;

//        OpenOptionMenu();
//    }

//    /// <summary>
//    /// 선택된 아이템 장착
//    /// </summary>
//    private void EquipSelectedItem()
//    {
//        if (selectedIndex < 0)
//            return;

//        if (items[selectedIndex] == null)
//            return;

//        ToolData toolData = ItemLoadManager.Instance.GetToolData(items[selectedIndex].itemId);

//        if (toolData == null)
//            return;

//        equip.EquipTool(selectedIndex);
//        CloseOptionMenu();
//    }

//    /// <summary>
//    /// 아이템 드롭하기
//    /// </summary>
//    /// <param name="index"></param>
//    private void DropItem(int index)
//    {
//        // index 범위 초과하면 리턴
//        if (index < 0 || index >= items.Length)
//            return;

//        InventoryItem item = items[index];

//        if (item == null)
//            return;

//        GameObject itemPrefab = ItemLoadManager.Instance.GetItemPrefab(item.itemId);

//        if (itemPrefab == null)
//        {
//            Debug.Log($"프리팹이 없습니다.\n{item.itemId}");
//            return;
//        }

//        // 장착 아이템이면 장착 해제
//        if (equip.IsEquippedItem(item))
//            equip.UnEquipTool();

//        Instantiate(itemPrefab, dropPoint.position, Quaternion.identity);

//        items[index] = null;
//        RefreshSlot(index);
//    }

//    /// <summary>
//    /// (버튼) 아이템 드롭하기
//    /// </summary>
//    private void DropSelectedItem()
//    {
//        if (selectedIndex < 0)
//            return;

//        DropItem(selectedIndex);
//        CloseOptionMenu();
//    }
//} 
