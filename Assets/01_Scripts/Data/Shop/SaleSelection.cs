public sealed class SaleSelection
{
    public int SlotIndex { get; }
    public InventoryItem Item { get; }

    public string ItemId { get; }

    // 현재 보유 수량 기록
    public int SourceCount { get; }
    public int SourceDurability { get; }

    // 판매할 수량 기록
    public int Count { get; }
    
    // 개당 판매 가격 기록
    public int UnitPrice { get; }

    public int Total => Count * UnitPrice;

    public SaleSelection(
        int slotIndex,
        InventoryItem item,
        int count,
        int unitPrice)
    {
        SlotIndex = slotIndex;
        Item = item;

        ItemId = item.itemId;

        SourceCount = item.count;
        SourceDurability = item.currentDurability;

        Count = count;
        UnitPrice = unitPrice;
    }
}