using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Game/Shop Data")]
public class ShopData : ScriptableObject
{
    public string shopName;

    [Header("Buy")]
    public string[] buyItemIds;

    [Header("Sell")]
    public string[] sellItemIds;

    /// <summary>
    /// 상점 구매 가능 여부
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool CanBuy(string itemId)
    {
        return ContainsId(buyItemIds, itemId);
    }

    /// <summary>
    /// 상점 판매 가능 여부
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool CanSell(string itemId)
    {
        return ContainsId(sellItemIds, itemId);
    }

    /// <summary>
    /// ID 비교
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    private bool ContainsId(string[] ids, string itemId)
    {
        if (ids == null || string.IsNullOrEmpty(itemId))
            return false;

        foreach (string id in ids)
        {
            if (id == itemId)
                return true;
        }

        return false;
    }
}
