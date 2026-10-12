using UnityEngine;

public class SaleSelection
{
    public int SlotIndex {  get; }
    public InventoryItem Item { get; }

    public string ItemId { get; }
    public int Count { get; }
    public int UnitPrice { get; }

    public int Total => Count * UnitPrice;

    public SaleSelection(int slotIndex, InventoryItem item, int unitPrice)
    {
        SlotIndex = slotIndex;
        Item = item;

        ItemId = item.itemId;
        Count = item.count;
        UnitPrice = unitPrice;
    }
}
