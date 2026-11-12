using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상품 ID & 하루 거래 한도
/// </summary>
[Serializable]
public class ShopItemEntry
{
    public string itemId;

    [Tooltip("-1: 무제한 / 0: 거래 불가 / 양수: 하루 거래 수량")]
    [Min(-1)] public int dailyLimit = -1;
}

[CreateAssetMenu(fileName = "ShopData", menuName = "Game/Shop Data")]
public class ShopData : ScriptableObject
{
    [Header("상점 정보")]
    public string shopId;
    public string shopName;

    [Header("구매할 수 있는 상품")]
    public ShopItemEntry[] buyItems = Array.Empty<ShopItemEntry>();

    /// <summary>
    /// 구매 목록에서 아이템 찾아 반환
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public ShopItemEntry GetBuyEntry(string itemId)
    {
        return FindEntry(buyItems, itemId);
    }

    /// <summary>
    /// 아이템이 구매 목록에 있는지 확인
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public bool CanBuy(string itemId)
    {
        return GetBuyEntry(itemId) != null;
    }

    /// <summary>
    /// 상품 검색
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    private static ShopItemEntry FindEntry(ShopItemEntry[] entries, string itemId)
    {
        if (entries == null || string.IsNullOrWhiteSpace(itemId))
            return null;

        foreach (ShopItemEntry entry in entries)
        {
            if (entry != null && entry.itemId == itemId)
                return entry;
        }

        return null;
    }

    /// <summary>
    /// 상점 데이터 검사
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(shopId))
        {
            error = "상점 ID가 비어 있습니다.";
            return false;
        }

        return ValidateEntries(buyItems, "구매", out error);
    }

    /// <summary>
    /// 상품 목록 검사
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="label"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private static bool ValidateEntries(
        ShopItemEntry[] entries,
        string label,
        out string error)
    {
        error = "";

        if (entries == null)
            return true;

        HashSet<string> ids = new();

        foreach (ShopItemEntry entry in entries)
        {
            if (entry == null ||
                string.IsNullOrWhiteSpace(entry.itemId))
            {
                error = $"{label} 목록에 비어 있는 상품이 있습니다.";
                return false;
            }

            if (!ids.Add(entry.itemId))
            {
                error = $"{label} 목록에 중복 ID가 있습니다: {entry.itemId}";
                return false;
            }

            if (entry.dailyLimit < -1)
            {
                error = $"{entry.itemId}의 한도는 -1 이상이어야 합니다.";
                return false;
            }
        }

        return true;
    }
}
