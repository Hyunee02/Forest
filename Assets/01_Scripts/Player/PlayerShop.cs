using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerShop : MonoBehaviour
{
    [Header("현재 소지금")]
    [SerializeField, Min(0)] private int money = 10000;

    private PlayerInventory inventory;
    private PlayerEquip equip;

    private readonly Dictionary<(string shopId, string itemId), int>
        boughtAmounts = new();

    private bool bTrading;

    public PlayerInventory Inventory => inventory;
    public bool IsReady => inventory != null;
    public bool IsUsingTool => equip != null && equip.BUse;
    public int Money => money;
    public bool IsTrading => bTrading;

    // 기존 ShopUI가 날짜 변경을 확인할 때 사용한다.
    public int Day => 1;

    public event System.Action TradeCompleted;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        equip = GetComponent<PlayerEquip>();
    }

    public void RestoreMoney(int amount)
    {
        money = amount;
    }

    public long GetOwnedCount(string itemId)
    {
        long total = 0;

        for (int i = 0; i < inventory.SlotCount; i++)
        {
            InventoryItem item = inventory.GetItem(i);

            if (item != null && !item.BEmpty && item.itemId == itemId)
                total += item.count;
        }

        return total;
    }

    public int GetBuyRemaining(ShopData market, string itemId)
    {
        if (!IsReady || market == null)
            return 0;

        ShopItemEntry entry = market.GetBuyEntry(itemId);

        if (entry == null)
            return 0;

        if (entry.dailyLimit < 0)
            return int.MaxValue;

        boughtAmounts.TryGetValue((market.shopId, itemId), out int bought);

        return Mathf.Max(0, entry.dailyLimit - bought);
    }

    private bool CanTrade(out string message)
    {
        if (!IsReady)
        {
            message = "플레이어 인벤토리를 찾을 수 없습니다.";
            return false;
        }

        if (bTrading || IsUsingTool)
        {
            message = "지금은 거래할 수 없습니다.";
            return false;
        }

        message = "";
        return true;
    }

    public bool TryBuy(
        ShopData market,
        string itemId,
        int amount,
        out string message)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            message = "구매할 상품을 확인해주세요.";
            return false;
        }

        Dictionary<string, int> selection = new()
        {
            { itemId, amount }
        };

        return TryBuySelected(market, selection, out message);
    }

    public bool TryBuySelected(
        ShopData market,
        IReadOnlyDictionary<string, int> selections,
        out string message)
    {
        if (!CanTrade(out message))
            return false;

        if (market == null)
        {
            message = "상점 데이터가 연결되지 않았습니다.";
            return false;
        }

        if (!market.Validate(out message))
            return false;

        if (selections == null || selections.Count == 0)
        {
            message = "구매할 상품을 선택해주세요.";
            return false;
        }

        // 인벤토리 변경으로 UI가 갱신돼도 이번 구매 목록을 유지한다.
        Dictionary<string, int> transaction = new();

        long totalPrice = 0;
        long totalCount = 0;

        foreach (KeyValuePair<string, int> selection in selections)
        {
            string itemId = selection.Key;
            int amount = selection.Value;

            if (amount <= 0)
            {
                message = "구매 수량을 확인해주세요.";
                return false;
            }

            if (!market.CanBuy(itemId))
            {
                message = "이 상점에서 구매할 수 없는 상품입니다.";
                return false;
            }

            ItemData_SO data = inventory.GetItemData(itemId);

            if (data == null || data.BuyPrice < 0)
            {
                message = $"구매 정보를 확인할 수 없습니다. ({itemId})";
                return false;
            }

            if (data.ItemType == ItemTypeSO.Tool)
            {
                ToolData tool = inventory.GetToolData(itemId);

                if (tool == null || tool.Durability <= 0)
                {
                    message = $"도구 데이터를 확인해주세요. ({itemId})";
                    return false;
                }
            }

            if (amount > GetBuyRemaining(market, itemId))
            {
                message = $"{data.ItemName}의 구매 가능 수량을 초과했습니다.";
                return false;
            }

            long price = (long)data.BuyPrice * amount;

            if (price > (long)money - totalPrice)
            {
                message = "소지금이 부족합니다.";
                return false;
            }

            totalPrice += price;
            totalCount += amount;
            transaction.Add(itemId, amount);
        }

        bTrading = true;

        try
        {
            money -= (int)totalPrice;
            ChangeBoughtAmounts(market, transaction, 1);

            if (!inventory.TryAddItemsBatch(transaction, out message))
            {
                money += (int)totalPrice;
                ChangeBoughtAmounts(market, transaction, -1);
                return false;
            }

            message = $"{totalCount:N0}개를 {totalPrice:N0} G에 구매했습니다.";
        }
        finally
        {
            bTrading = false;
        }

        TradeCompleted?.Invoke();
        return true;
    }

    private void ChangeBoughtAmounts(
        ShopData market,
        Dictionary<string, int> transaction,
        int direction)
    {
        foreach (KeyValuePair<string, int> purchase in transaction)
        {
            ShopItemEntry entry = market.GetBuyEntry(purchase.Key);

            if (entry.dailyLimit < 0)
                continue;

            var key = (market.shopId, purchase.Key);
            boughtAmounts.TryGetValue(key, out int previous);

            int next = previous + purchase.Value * direction;

            if (next == 0)
                boughtAmounts.Remove(key);
            else
                boughtAmounts[key] = next;
        }
    }

    public bool TrySellSelected(
        IReadOnlyList<SaleSelection> selections,
        out string message)
    {
        if (!CanTrade(out message))
            return false;

        if (!inventory.ValidateSaleSelections(selections))
        {
            message = "판매할 아이템과 수량을 다시 선택해주세요.";
            return false;
        }

        List<SaleSelection> transaction = new(selections);
        long totalPrice = 0;

        foreach (SaleSelection selection in transaction)
        {
            ItemData_SO data = inventory.GetItemData(selection.ItemId);

            if (data == null || !data.canSell || data.SellPrice < 0)
            {
                message = "판매할 수 없는 아이템입니다.";
                return false;
            }

            if (selection.UnitPrice != data.SellPrice)
            {
                message = "판매 가격이 변경되었습니다. 다시 선택해주세요.";
                return false;
            }

            long price = (long)data.SellPrice * selection.Count;

            if (price > (long)int.MaxValue - money - totalPrice)
            {
                message = "판매 후 소지금이 최대치를 초과합니다.";
                return false;
            }

            totalPrice += price;
        }

        bTrading = true;

        try
        {
            money += (int)totalPrice;

            if (!inventory.RemoveSaleSelections(transaction))
            {
                money -= (int)totalPrice;
                message = "아이템이 변경되었습니다. 다시 선택해주세요.";
                return false;
            }

            message = $"{totalPrice:N0} G에 판매했습니다.";
        }
        finally
        {
            bTrading = false;
        }

        TradeCompleted?.Invoke();
        return true;
    }
}