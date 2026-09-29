using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("<< Database >>")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("<< Inventory >>")]
    [SerializeField] private int slotCount = 16;
    [SerializeField, Min(1)] private int maxSlotCount;
    [SerializeField, Min(1)] private int upgradeSlotCount;

    private List<InventoryItem> items;

    private PlayerEquip equip;

    public event Action OnChanged;

    public int SlotCount
    {
        get
        {
            Initialize();
            return items.Count;
        }
    }

    public int MaxSlotCount
    {
        get
        {
            Initialize();
            return maxSlotCount;
        }
    }

    public bool CanUpgrade => SlotCount < MaxSlotCount;

#if UNITY_EDITOR
    private void Reset()
    {
        maxSlotCount = 48;
        upgradeSlotCount = 8;
    }
#endif

    private void Awake()
    {
        Initialize();

        equip = GetComponent<PlayerEquip>();
    }

    /// <summary>
    /// 인벤토리 아이템 개수 초기화
    /// </summary>
    private void Initialize()
    {
        if (items != null)
            return;

        slotCount = Mathf.Max(1, slotCount);
        maxSlotCount = Mathf.Max(slotCount, maxSlotCount);
        upgradeSlotCount = Mathf.Max(1, upgradeSlotCount);

        items = new List<InventoryItem>(maxSlotCount);

        // 인벤토리 아이템 리스트 생성
        for (int i = 0; i < slotCount; i++)
            items.Add(new InventoryItem());
    }

    #region > 데이터 가져오기
    /// <summary>
    /// 특정 슬롯 아이템 가져오기
    /// </summary>
    public InventoryItem GetItem(int slotIndex)
    {
        Initialize();

        if (slotIndex < 0 || slotIndex >= items.Count)
            return null;

        return items[slotIndex];
    }

    /// <summary>
    /// 아이템 데이터 가져오기
    /// </summary>
    public ItemData_SO GetItemData(string itemId)
    {
        if (itemDatabase == null)
            return null;

        return itemDatabase.GetItemData(itemId);
    }

    /// <summary>
    /// 도구 데이터 가져오기
    /// </summary>
    public ToolData GetToolData(string itemId)
    {
        if (itemDatabase == null)
            return null;

        return itemDatabase.GetToolData(itemId);
    }
    #endregion

    #region > 아이템 확인
    /// <summary>
    /// 소유한 인벤토리 아이템인지 확인
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool Contains(InventoryItem target)
    {
        Initialize();

        if (target == null || target.BEmpty)
            return false;

        foreach (InventoryItem item in items)
        {
            // 객체가 같으면
            if (ReferenceEquals(item, target))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 아이템 최대 스택 확인
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private int GetMaxStack(ItemData_SO data)
    {
        // 도구면 MaxStack 1
        if (data.itemType == ItemTypeSO.Tool)
            return 1;

        return Mathf.Max(1, data.maxStack);
    }

    /// <summary>
    /// 요청한 수량 전체가 들어갈 수 있는지 검사
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool CanAddItem(string itemId, int amount = 1)
    {
        Initialize();

        if (amount <= 0)
            return false;

        ItemData_SO data = GetItemData(itemId);

        if (data == null)
            return false;

        // 아이템이 도구이면
        bool bTool = data.itemType == ItemTypeSO.Tool;

        if (bTool)
        {
            ToolData tool = GetToolData(itemId);

            if (tool == null || tool.Durability <= 0)
                return false;
        }

        // 최대 수량
        int maxStack = GetMaxStack(data);

        // 추가해야 하는 아이템 수량
        int remain = amount;

        foreach (InventoryItem item in items)
        {
            // 남은 공간
            int space = 0;

            // 아이템이 없으면 최대 스택만큼 추가 가능
            if (item.BEmpty)
                space = maxStack;

            else if (!bTool && item.itemId == itemId)
                space = Mathf.Max(0, maxStack - item.count);

            // 들어간 만큼 빼줌
            remain -= Mathf.Min(remain, space);

            if (remain == 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 사용중인 아이템인지 확인
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool IsUsingItem(InventoryItem item)
    {
        return equip != null
            && equip.BUse
            && ReferenceEquals(item, equip.CurInventoryItem);
    }
    #endregion

    #region > 아이템 정보 변경
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(string itemId, int amount = 1, int durability = 0)
    {
        if (!CanAddItem(itemId, amount))
            return false;

        ItemData_SO data = GetItemData(itemId);
        bool bTool = data.itemType == ItemTypeSO.Tool;
        int maxStack = GetMaxStack(data);

        // 추가하려는 아이템이 도구일 때
        if (bTool)
        {
            ToolData tool = GetToolData(itemId);

            // 내구도가 0보다 작거나 같으면 최대 내구도로 추가
            if (durability <= 0)
                durability = tool.Durability;

            durability = Mathf.Clamp(durability, 0, tool.Durability);

            if (durability == 0)
                return false;
        }

        else
            durability = 0;

        // 요청받은 수량
        int remain = amount;

        // 기존 아이템 남은 스택 채우기
        if (!bTool)
        {
            foreach (InventoryItem item in items)
            {
                if (item.BEmpty || item.itemId != itemId)
                    continue;

                // 들어갈 수 있는 공간
                int space = Mathf.Max(0, maxStack - item.count);
                // 추가할 수량
                int addCount = Mathf.Min(remain, space);

                item.count += addCount;
                remain -= addCount;

                if (remain == 0)
                    break;
            }
        }

        // 남은 수량 빈 슬롯에 넣기
        if (remain > 0)
        {
            foreach (InventoryItem item in items)
            {
                if (!item.BEmpty)
                    continue;

                int addCount = Mathf.Min(remain, maxStack);

                item.Set(itemId, addCount, durability);
                remain -= addCount;

                if (remain == 0)
                    break;
            }
        }

        NotifyChanged();

        return true;
    }

    /// <summary>
    /// 아이템 삭제
    /// </summary>
    public bool RemoveItem(int slotIndex, int amount = 1)
    {
        InventoryItem item = GetItem(slotIndex);

        if (item == null || item.BEmpty || amount <= 0)
            return false;

        if (item.count < amount || IsUsingItem(item))
            return false;

        item.count -= amount;

        if (item.count == 0)
            item.Clear();

        NotifyChanged();

        return true;
    }

    /// <summary>
    /// 아이템 이동
    /// </summary>
    public void MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex)
            return;

        InventoryItem from = GetItem(fromIndex);
        InventoryItem to = GetItem(toIndex);

        if (from == null || to == null || from.BEmpty)
            return;

        if (IsUsingItem(from) || IsUsingItem(to))
            return;

        ItemData_SO data = GetItemData(from.itemId);

        if (data == null)
            return;

        bool bMerge = !to.BEmpty
            && from.itemId == to.itemId
            && data.itemType != ItemTypeSO.Tool;

        // 합병 가능할 때
        if (bMerge)
        {
            int space = Mathf.Max(0, GetMaxStack(data) - to.count);

            if (space == 0)
                return;

            int moveCount = Mathf.Min(from.count, space);

            to.count += moveCount;
            from.count -= moveCount;

            if (from.count == 0)
                from.Clear();
        }

        // 객체 교환
        else
        {
            items[fromIndex] = to;
            items[toIndex] = from;
        }

        NotifyChanged();
    }
    #endregion

    #region > 업그레이드
    /// <summary>
    /// 슬롯 개수 늘리기
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool ExpandSlots(int amount)
    {
        Initialize();

        if (amount <= 0 || !CanUpgrade)
            return false;

        int addCount = Mathf.Min(amount, maxSlotCount - items.Count);

        for (int i = 0; i < addCount; i++)
            items.Add(new InventoryItem());

        NotifyChanged();

        return true;
    }

    /// <summary>
    /// 업그레이드 1회 : 설정한 칸 수만큼 증가
    /// </summary>
    /// <returns></returns>
    public bool Upgrade()
    {
        return ExpandSlots(upgradeSlotCount);
    }
    #endregion

    /// <summary>
    /// 내구도 감소
    /// </summary>
    public bool ReduceDurability(InventoryItem item, int amount)
    {
        if (!Contains(item) || amount <= 0)
            return false;

        ItemData_SO data = GetItemData(item.itemId);

        if (data == null || data.itemType != ItemTypeSO.Tool)
            return false;

        item.currentDurability = Mathf.Max(0, item.currentDurability - amount);

        bool remain = item.currentDurability > 0;

        if (!remain)
            item.Clear();

        NotifyChanged();

        return remain;
    }

    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}