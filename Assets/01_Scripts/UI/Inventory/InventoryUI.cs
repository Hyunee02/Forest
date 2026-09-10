using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("<< Inventory >>")]
    [SerializeField] private PlayerInventory inventory;

    [Header("<< Slots >>")]
    [SerializeField] private InventorySlot[] slots;

    [Header("<< Item UI >>")]
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("<< Gold >>")]
    [SerializeField] private GameObject goldText;

    private void Start()
    {
        Refresh(inventory);
    }

    public void Refresh(PlayerInventory playerInventory)
    {
        if (playerInventory == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            ClearSlot(slots[i]);

            InventoryItem item = playerInventory.GetItem(i);

            if (item == null || item.BEmpty)
                continue;

            ItemData_SO itemData = playerInventory.GetItemData(item.itemId);

            if (itemData == null)
                continue;

            GameObject itemObject =
                Instantiate(
                    inventoryItemPrefab,
                    slots[i].transform
                );

            InventoryItemUI itemUI = itemObject.GetComponent<InventoryItemUI>();

            itemUI.SetItem(itemData, item.count);

            DraggableItem draggable = itemObject.GetComponent<DraggableItem>();

            draggable.SetSlotIndex(i);
        }
    }

    private void ClearSlot(InventorySlot slot)
    {
        for (int i = slot.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(slot.transform.GetChild(i).gameObject);
        }
    }
}