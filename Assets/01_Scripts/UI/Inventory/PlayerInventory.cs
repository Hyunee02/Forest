using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("<< Database >>")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("<< Inventory >>")]
    [SerializeField] private int slotCount = 20;

    private InventoryItem[] items;

    public int SlotCount => slotCount;

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 인벤토리 초기화
    /// </summary>
    private void Initialize()
    {
        items = new InventoryItem[slotCount];

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new InventoryItem();
        }
    }

    /// <summary>
    /// 특정 슬롯 아이템 가져오기
    /// </summary>
    public InventoryItem GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Length)
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
    public ToolData_SO GetToolData(string itemId)
    {
        if (itemDatabase == null)
            return null;

        return itemDatabase.GetToolData(itemId);
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(
        string itemId,
        int amount = 1,
        int durability = 0)
    {
        if (string.IsNullOrEmpty(itemId))
            return false;

        if (amount <= 0)
            return false;

        ItemData_SO itemData = GetItemData(itemId);

        if (itemData == null)
        {
            Debug.LogWarning($"아이템 데이터를 찾을 수 없습니다 : {itemId}");
            return false;
        }

        int maxStack = Mathf.Max(1, itemData.maxStack);

        // Tool은 기본적으로 하나씩 별도 슬롯 사용
        bool isTool = itemData.itemType == ItemTypeSO.Tool;

        // --------------------------------------------------
        // 1. 기존 스택에 추가
        // --------------------------------------------------

        if (!isTool)
        {
            for (int i = 0; i < items.Length; i++)
            {
                InventoryItem item = items[i];

                if (item.BEmpty)
                    continue;

                if (item.itemId != itemId)
                    continue;

                if (item.count >= maxStack)
                    continue;

                int space = maxStack - item.count;
                int addAmount = Mathf.Min(amount, space);

                item.count += addAmount;
                amount -= addAmount;

                if (amount <= 0)
                {
                    RefreshUI();
                    return true;
                }
            }
        }

        // --------------------------------------------------
        // 2. 빈 슬롯에 추가
        // --------------------------------------------------

        while (amount > 0)
        {
            int emptySlotIndex = FindEmptySlot();

            if (emptySlotIndex == -1)
            {
                Debug.Log("인벤토리가 가득 찼습니다.");

                RefreshUI();
                return false;
            }

            int addAmount = isTool
                ? 1
                : Mathf.Min(amount, maxStack);

            items[emptySlotIndex].Set(
                itemId,
                addAmount,
                durability
            );

            amount -= addAmount;
        }

        RefreshUI();
        return true;
    }

    /// <summary>
    /// 빈 슬롯 찾기
    /// </summary>
    private int FindEmptySlot()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].BEmpty)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// 아이템 삭제
    /// </summary>
    public bool RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= items.Length)
            return false;

        InventoryItem item = items[slotIndex];

        if (item.BEmpty)
            return false;

        item.count -= amount;

        if (item.count <= 0)
            item.Clear();

        RefreshUI();

        return true;
    }

    /// <summary>
    /// 두 슬롯 아이템 위치 교환
    /// </summary>
    public void MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= items.Length)
            return;

        if (toIndex < 0 || toIndex >= items.Length)
            return;

        if (fromIndex == toIndex)
            return;

        InventoryItem temp = items[fromIndex];

        items[fromIndex] = items[toIndex];
        items[toIndex] = temp;

        RefreshUI();
    }

    /// <summary>
    /// 도구 내구도 감소
    /// </summary>
    public bool ReduceDurability(
        InventoryItem item,
        int amount)
    {
        if (item == null || item.BEmpty)
            return false;

        item.currentDurability -= amount;

        if (item.currentDurability <= 0)
        {
            item.Clear();

            RefreshUI();
            return false;
        }

        RefreshUI();
        return true;
    }

    private void RefreshUI()
    {
        InventoryUI inventoryUI =
            FindFirstObjectByType<InventoryUI>();

        if (inventoryUI != null)
            inventoryUI.Refresh(this);
    }
}