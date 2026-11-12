using System;

/// <summary>
/// 하루 거래 기록
/// </summary>
[Serializable]
public class ShopDailyRecord
{
    public string shopId;
    public string itemId;
    public int bought;
}
