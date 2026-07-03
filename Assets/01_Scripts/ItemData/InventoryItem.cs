using System;

[Serializable]
public class InventoryItem
{
    public string itemId;
    public int count;
    public int currentDurability;

    public InventoryItem(string itemId, int count, int currentDurability)
    {
        this.itemId = itemId;
        this.count = count;
        this.currentDurability = currentDurability;
    }
}
