using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("----- UI -----")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotParent;
    [SerializeField] private Image dragIcon;

    [Header("----- Option -----")]
    [SerializeField] private int maxItemCount = 24;

    private List<InventorySlot> slots;
    private List<InventoryItem> items;

    private PlayerBindInput input;
    private PlayerEquip equip;

    private InventorySlot dragStartSlot;

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();
        equip = GetComponent<PlayerEquip>();

        InitSlots();
        InitItems();

        dragIcon.gameObject.SetActive(false);
        dragIcon.raycastTarget = false;

        inventoryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        input.OnInventoryInput += ToggleInventory;
    }

    private void OnDisable()
    {
        input.OnInventoryInput -= ToggleInventory;
    }

    // 인벤토리 패널 온 / 오프
    private void ToggleInventory()
    {
        bool active = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(active);

        if (!active)
            EndDrag();
    }

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    private void InitSlots()
    {
        // 슬롯 리스트
        slots = new List<InventorySlot>();
        slots.Clear();

        // 비활성화 슬롯들까지 포함해서 찾기
        InventorySlot[] findSlots = slotParent.GetComponentsInChildren<InventorySlot>(true);

        // 슬롯 초기화
        for (int i = 0; i < findSlots.Length; i++)
        {
            findSlots[i].Init(this, i);
            slots.Add(findSlots[i]);
        }
    }

    /// <summary>
    /// 아이템 초기화
    /// </summary>
    private void InitItems()
    {
        items = new List<InventoryItem>();
        items.Clear();

        // 아이템 추가
        for (int i = 0; i < slots.Count; i++)
            items.Add(new InventoryItem());
    }

    /// <summary>
    /// 아이템 인덱스 가져오기
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public InventoryItem GetItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return null;

        return items[index];
    }

    /// <summary>
    /// 아이템 추가하기
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool AddItem(string itemId, int amount)
    {
        ItemData itemData = ItemLoadManager.Instance.GetItemData(itemId);
        ToolData toolData = ItemLoadManager.Instance.GetToolData(itemId);

        if (itemData == null || amount <= 0)
            return false;

        // 도구이면 1개, 아이템이면 maxStack
        int maxStack = toolData != null ? 1 : Mathf.Max(1, itemData.maxStack);

        // 인벤토리 빈 공간 검사
        if (GetEmptySpace(itemId, maxStack) < amount)
        {
            Debug.Log("인벤토리 공간이 부족합니다.");
            return false;
        }

        int remain = amount;

        // 아이템일 때, 기존 스택에 먼저 추가
        if (maxStack > 1)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].itemId != itemId)
                    continue;

                int space = maxStack - items[i].count;
                int addCount = Mathf.Min(space, remain);

                items[i].count += addCount;
                remain -= addCount;

                if (remain <= 0)
                    break;
            }
        }

        // 빈 슬롯에 새로 추가
        for (int i = 0; i < items.Count && remain > 0; i++)
        {
            if (!items[i].BEmpty)
                continue;

            int addCount = Mathf.Min(maxStack, remain);
            int durability = toolData == null ? 0 : toolData.durability;

            items[i].Set(itemId, addCount, durability);
            remain -= addCount;
        }

        RefreshAllSlots();

        return true;
    }

    /// <summary>
    /// 인벤토리 빈 공간 검사
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="maxStack">최대 아이템 갯수</param>
    /// <returns></returns>
    private int GetEmptySpace (string itemId, int maxStack)
    {
        int space = 0;

        for (int i = 0; i < items.Count; i++)
        {
            // 아이템이 없으면
            if (items[i].BEmpty)
                space += maxStack;

            // 아이템 아이디가 같으면
            else if (items[i].itemId == itemId && maxStack > 1)
                space += Mathf.Max(maxStack - items[i].count);
        }

        return space;
    }

    /// <summary>
    /// 드래그 시작
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="eventData"></param>
    public void BeginDrag(InventorySlot slot, PointerEventData eventData)
    {
        // 드래그 슬롯의 아이템 가져오기
        InventoryItem item = GetItem(slot.Index);

        // 빈 슬롯 드래그 방지
        if (item == null || item.BEmpty)
            return;

        // 시작 슬롯
        dragStartSlot = slot;

        // 드래그 아이콘 표시
        dragIcon.sprite = slot.GetIcon();
        dragIcon.gameObject.SetActive(true);
        dragIcon.transform.position = eventData.position;
    }

    /// <summary>
    /// 드래그 중
    /// </summary>
    /// <param name="evenData"></param>
    public void Drag(PointerEventData evenData)
    {
        if (dragStartSlot == null)
            return;

        dragIcon.transform.position = evenData.position;
    }

    /// <summary>
    /// 드롭
    /// </summary>
    /// <param name="dropSlot"></param>
    public void Drop(InventorySlot dropSlot)
    {
        if (dragStartSlot == null)
            return;

        MoveOrMerge(dragStartSlot.Index, dropSlot.Index);
    }

    /// <summary>
    /// 드래그 종료
    /// </summary>
    public void EndDrag()
    {
        dragStartSlot = null;
        dragIcon.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 드래그 및 아이템 합치기
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="dropIndex"></param>
    private void MoveOrMerge(int startIndex, int dropIndex)
    {
        // 같은 슬롯이면 반응 X
        if (startIndex == dropIndex)
            return;
        
        InventoryItem startItem = items[startIndex];
        InventoryItem dropItem = items[dropIndex];

        ItemData itemData = ItemLoadManager.Instance.GetItemData(startItem.itemId);
        ToolData toolData = ItemLoadManager.Instance.GetToolData(startItem.itemId);

        int maxStack = toolData != null ? 1 : Mathf.Max(1, itemData.maxStack);

        bool sameItem = !dropItem.BEmpty && startItem.itemId == dropItem.itemId;

        // 같은 아이템이고 스택 비어있으면 합치기
        if (sameItem && maxStack > 1)
        {
            int space = maxStack - dropItem.count;
            int moveCount = Mathf.Min(space, startItem.count);

            dropItem.count += moveCount;
            startItem.count -= moveCount;

            if (startItem.count <= 0)
                startItem.Clear();
        }

        // 못 합치면 자리 교체
        else
        {
            items[startIndex] = dropItem;
            items[dropIndex] = startItem;
        }

        // UI 갱신
        RefreshSlot(startIndex);
        RefreshSlot(dropIndex);
    }

    /// <summary>
    /// 아이템 사용
    /// </summary>
    /// <param name="index"></param>
    public void UseItem(int index)
    {
        InventoryItem item = GetItem(index);

        // 아이템 null 방지
        if (item == null || item.BEmpty)
            return;

        ToolData toolData = ItemLoadManager.Instance.GetToolData(item.itemId);

        // 도구면 장착 실행
        if (toolData != null)
            equip.EquipTool(index);
    }

    /// <summary>
    /// 내구도 감소
    /// </summary>
    /// <param name="targetItem"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool ReduceDurability(InventoryItem targetItem, int amount)
    {
        // 아이템이 몇 번째에 있는지 찾기
        int index = items.IndexOf(targetItem);

        if (index < 0 || targetItem.BEmpty)
            return false;

        targetItem.currentDurability -= amount;

        // 내구도 0 이하면 아이템 파괴
        if (targetItem.currentDurability <= 0)
        {
            targetItem.Clear();
            RefreshSlot(index);

            return false;
        }

        RefreshSlot(index);

        return true; 
    }

    /// <summary>
    /// 전체 슬롯 갱신
    /// </summary>
    private void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Count; i++)
            RefreshSlot(i);
    }

    /// <summary>
    /// 슬롯 갱신
    /// </summary>
    /// <param name="index"></param>
    private void RefreshSlot(int index)
    {
        InventoryItem item = items[index];

        // 아이템 비어있으면 슬롯 비우기
        if (item.BEmpty)
        {
            slots[index].Clear();
            return;
        }

        // 아이콘, 데이터 가져오기
        Sprite sprite = ItemLoadManager.Instance.GetItemSprite(item.itemId);
        ToolData toolData = ItemLoadManager.Instance.GetToolData(item.itemId);

        // 슬롯 UI 갱신
        slots[index].SetItem(sprite, item.count, toolData, item.currentDurability);
    }
} 
