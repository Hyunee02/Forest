using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class InventoryTest : MonoBehaviour
{
    [Serializable]
    public class GiveItemEntry
    {
        public ItemData_SO itemData;

        [Min(1)] public int amount = 1;

        [Tooltip("-1이면 최대 내구도, 양수면 지정한 내구도")]
        public int durability = -1;
    }

    [Serializable]
    public class SlotInfo
    {
        public int slotIndex;
        public string itemId;
        public string itemName;
        public int count;
        public int currentDurability;
    }

    [Header("지급 아이템")]
    [SerializeField] private List<GiveItemEntry> giveItems = new();

    [Header("인벤토리 실행 중 확인")]
    [SerializeField] private int currentSlotCount;

    [SerializeField] List<SlotInfo> currentItems = new();

    private PlayerInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        inventory.OnChanged += RefreshView;
    }

    private void Start()
    {
        RefreshView();
    }

    private void OnDisable()
    {
        inventory.OnChanged -= RefreshView;
    }

    [ContextMenu("테스트/지정 아이템 지급")]
    private void GiveItems()
    {
        foreach (GiveItemEntry entry in giveItems)
        {
            if (entry == null || entry.itemData == null)
                continue;

            bool success = inventory.AddItem(entry.itemData.Id, entry.amount, entry.durability);

            if (success)
                Debug.Log($"{entry.itemData.ItemName}, {entry.amount}개 지급 완료", this);

            else
                Debug.Log($"{entry.itemData.ItemName} 지금 실패" +
                    $"ID : {entry.itemData.Id}");
        }

        RefreshView();
    }

    [ContextMenu("테스트/인벤토리 목록 새로고침")]
    private void RefreshView()
    {
        currentSlotCount = inventory.SlotCount;
        currentItems.Clear();

        for (int i = 0; i < currentSlotCount; i++)
        {
            InventoryItem item = inventory.GetItem(i);

            SlotInfo info = new SlotInfo();
            info.slotIndex = i;

            if (item == null || item.BEmpty)
            {
                info.itemId = "";
                info.itemName = "(빈 슬롯)";
                info.count = 0;
                info.currentDurability = 0;
            }

            else
            {
                ItemData_SO data = inventory.GetItemData(item.itemId);

                info.itemId = item.itemId;
                info.itemName = data != null
                    ? data.ItemName : "데이터 없음";

                info.count = item.count;
                info.currentDurability = item.currentDurability;
            }

            currentItems.Add(info);
        }
    }
}
