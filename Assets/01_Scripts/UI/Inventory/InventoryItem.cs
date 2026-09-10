using System;

[Serializable]
public class InventoryItem
{
    public string itemId;
    public int count;
    public int currentDurability;

    public bool BEmpty =>
        string.IsNullOrEmpty(itemId) || count <= 0;

    /// <summary>
    /// 인벤토리 아이템 정보 설정
    /// </summary>
    public void Set(string id, int amount, int durability)
    {
        itemId = id;
        count = amount;
        currentDurability = durability;
    }

    /// <summary>
    /// 인벤토리 아이템 정보 초기화
    /// </summary>
    public void Clear()
    {
        itemId = "";
        count = 0;
        currentDurability = 0;
    }
}