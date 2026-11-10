using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerSave))]
public class PlayerShop : MonoBehaviour
{
    private PlayerInventory inventory;
    private PlayerEquip equip;
    private PlayerSave playerSave;

    private bool bTrading;

    public PlayerInventory Inventory => inventory;

    public bool IsReady =>
        playerSave != null && playerSave.IsReady;

    public bool IsUsingTool =>
        equip != null && equip.BUse;

    public int Money =>
        GameSession.Instance.CurrentData?.money ?? 0;

    public int Day =>
        GameSession.Instance.CurrentData?.day ?? 1;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        equip = GetComponent<PlayerEquip>();
        playerSave = GetComponent<PlayerSave>();
    }

    public long GetOwnedCount(string itemId)
    {
        long total = 0;

        for (int i = 0; i < inventory.SlotCount; i++)
        {
            InventoryItem item = inventory.GetItem(i);

            if (item != null &&
                !item.BEmpty &&
                item.itemId == itemId)
            {
                total += item.count;
            }
        }

        return total;
    }

    private ShopDailyRecord FindRecord(
        string shopId,
        string itemId)
    {
        GameSaveData save = GameSession.Instance.CurrentData;

        if (save == null || save.dailyTrades == null)
            return null;

        foreach (ShopDailyRecord record in save.dailyTrades)
        {
            if (record.shopId == shopId &&
                record.itemId == itemId)
            {
                return record;
            }
        }

        return null;
    }

    private ShopDailyRecord GetOrCreateRecord(
        string shopId,
        string itemId)
    {
        ShopDailyRecord record = FindRecord(shopId, itemId);

        if (record != null)
            return record;

        record = new ShopDailyRecord
        {
            shopId = shopId,
            itemId = itemId
        };

        GameSession.Instance.CurrentData.dailyTrades.Add(record);

        return record;
    }

    public int GetBuyRemaining(
        ShopData market,
        string itemId)
    {
        return GetRemaining(market, itemId, true);
    }

    public int GetSellRemaining(
        ShopData market,
        string itemId)
    {
        return GetRemaining(market, itemId, false);
    }

    private int GetRemaining(
        ShopData market,
        string itemId,
        bool buying)
    {
        if (!IsReady || market == null)
            return 0;

        ShopItemEntry entry = buying
            ? market.GetBuyEntry(itemId)
            : market.GetSellEntry(itemId);

        if (entry == null)
            return 0;

        ShopDailyRecord record =
            FindRecord(market.shopId, itemId);

        int used = record == null
            ? 0
            : buying ? record.bought : record.sold;

        // 무제한이어도 저장 카운터의 int 범위를 넘기지는 않음
        int limit = entry.dailyLimit < 0
            ? int.MaxValue
            : entry.dailyLimit;

        return Mathf.Max(0, limit - used);
    }

    private bool CheckReady(
        ShopData market,
        out string message)
    {
        message = "";

        if (!IsReady)
        {
            message = "플레이어 데이터를 준비 중입니다.";
            return false;
        }

        if (bTrading || IsUsingTool)
        {
            message = "지금은 거래할 수 없습니다.";
            return false;
        }

        if (market == null)
            return ConfigError("ShopData가 없습니다.", out message);

        if (!market.Validate(out string error))
            return ConfigError(error, out message);

        GameSaveData save = GameSession.Instance.CurrentData;

        if (save.money < 0 ||
            save.dailyTrades == null ||
            save.tradeDay != save.day)
        {
            return ConfigError(
                "소지금 또는 날짜별 거래 기록이 올바르지 않습니다.",
                out message);
        }

        return true;
    }

    private bool ConfigError(
        string detail,
        out string message)
    {
        Debug.LogError($"[PlayerShop] {detail}", this);

        message = "거래를 진행할 수 없습니다.";
        return false;
    }

    public bool TryBuy(
        ShopData market,
        string itemId,
        int amount,
        out string message)
    {
        if (!CheckReady(market, out message))
            return false;

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
            return ConfigError(
                $"상품 데이터 오류: {itemId}",
                out message);
        }

        if (data.ItemType == ItemTypeSO.Tool)
        {
            ToolData tool = inventory.GetToolData(itemId);

            if (tool == null || tool.Durability <= 0)
            {
                return ConfigError(
                    $"도구 데이터 오류: {itemId}",
                    out message);
            }
        }

        if (amount > GetBuyRemaining(market, itemId))
        {
            message = "오늘 구매할 수 있는 수량을 초과했습니다.";
            return false;
        }

        long total = (long)data.BuyPrice * amount;

        GameSaveData save = GameSession.Instance.CurrentData;

        if (total > save.money)
        {
            message = "소지금이 부족합니다.";
            return false;
        }

        if (!inventory.CanAddItem(itemId, amount))
        {
            message = "인벤토리 공간이 부족합니다.";
            return false;
        }

        bTrading = true;

        try
        {
            ShopDailyRecord record =
                GetOrCreateRecord(market.shopId, itemId);

            int previousMoney = save.money;
            int previousBought = record.bought;

            // 인벤토리 이벤트에서 변경된 거래 상태를 읽게 함
            save.money -= (int)total;
            record.bought += amount;

            if (!inventory.AddItem(itemId, amount))
            {
                save.money = previousMoney;
                record.bought = previousBought;

                message = "구매하지 못했습니다. 다시 시도해주세요.";
                return false;
            }

            message = $"{data.ItemName} {amount}개를 구매했습니다.";

            AppendSaveResult(ref message);
            return true;
        }
        finally
        {
            bTrading = false;
        }
    }

    public bool TryBuySelected(
    ShopData market,
    IReadOnlyDictionary<string, int> selections,
    out string message)
    {
        if (!CheckReady(market, out message))
            return false;

        if (selections == null || selections.Count == 0)
        {
            message = "구매할 상품을 선택해주세요.";
            return false;
        }

        // UI가 갱신되어 선택 목록이 변경되어도 영향을 받지 않도록 복사
        Dictionary<string, int> transaction = new();

        GameSaveData save = GameSession.Instance.CurrentData;

        long totalPrice = 0;
        long totalCount = 0;

        foreach (KeyValuePair<string, int> selection in selections)
        {
            string itemId = selection.Key;
            int amount = selection.Value;

            if (amount <= 0)
            {
                message = "구매 수량이 올바르지 않습니다.";
                return false;
            }

            if (!market.CanBuy(itemId))
            {
                message = "이 상점에서 구매할 수 없는 상품이 포함되어 있습니다.";
                return false;
            }

            ItemData_SO data = inventory.GetItemData(itemId);

            if (data == null || data.BuyPrice < 0)
            {
                return ConfigError(
                    $"상품 데이터 오류: {itemId}",
                    out message);
            }

            if (data.ItemType == ItemTypeSO.Tool)
            {
                ToolData tool = inventory.GetToolData(itemId);

                if (tool == null || tool.Durability <= 0)
                {
                    return ConfigError(
                        $"도구 데이터 오류: {itemId}",
                        out message);
                }
            }

            if (amount > GetBuyRemaining(market, itemId))
            {
                message =
                    $"{data.ItemName}의 오늘 구매 가능 수량을 초과했습니다.";

                return false;
            }

            long itemPrice = (long)data.BuyPrice * amount;

            // 더하기 전에 남은 예산과 비교
            if (itemPrice > (long)save.money - totalPrice)
            {
                message = "선택한 상품을 모두 구매할 소지금이 부족합니다.";
                return false;
            }

            totalPrice += itemPrice;
            totalCount += amount;

            transaction.Add(itemId, amount);
        }

        bTrading = true;

        try
        {
            int previousMoney = save.money;

            // 인벤토리 변경 이벤트가 새 돈과 한도를 읽도록 먼저 반영
            save.money -= (int)totalPrice;

            foreach (KeyValuePair<string, int> purchase in transaction)
            {
                GetOrCreateRecord(
                    market.shopId,
                    purchase.Key).bought += purchase.Value;
            }

            // 모든 상품이 들어가면 한 번에 추가
            if (!inventory.TryAddItemsBatch(
                    transaction,
                    out message))
            {
                // 아이템 추가 실패 시 돈과 거래 기록 복구
                save.money = previousMoney;

                foreach (KeyValuePair<string, int> purchase in transaction)
                {
                    GetOrCreateRecord(
                        market.shopId,
                        purchase.Key).bought -= purchase.Value;
                }

                return false;
            }

            message =
                $"{transaction.Count}종, 총 {totalCount:N0}개를 " +
                $"{totalPrice:N0} G에 구매했습니다.";

            AppendSaveResult(ref message);

            return true;
        }
        finally
        {
            bTrading = false;
        }
    }

    public bool TrySellSelected(
    IReadOnlyList<SaleSelection> selections,
    out string message)
    {
        message = "";

        if (!IsReady)
        {
            message = "플레이어 데이터를 준비 중입니다.";
            return false;
        }

        if (bTrading || IsUsingTool)
        {
            message = "지금은 거래할 수 없습니다.";
            return false;
        }

        GameSaveData save = GameSession.Instance.CurrentData;

        if (save == null || save.money < 0)
        {
            return ConfigError(
                "플레이어 소지금 데이터가 올바르지 않습니다.",
                out message);
        }

        if (!inventory.ValidateSaleSelections(selections))
        {
            message = "판매할 아이템과 수량을 다시 선택해주세요.";
            return false;
        }

        // 인벤토리 변경 이벤트가 UI 선택을 초기화해도
        // 거래에 사용할 목록은 유지됨
        List<SaleSelection> transaction = new(selections);

        long total = 0;

        foreach (SaleSelection selection in transaction)
        {
            ItemData_SO data =
                inventory.GetItemData(selection.ItemId);

            if (data == null || data.SellPrice < 0)
            {
                return ConfigError(
                    $"판매 데이터가 올바르지 않습니다: {selection.ItemId}",
                    out message);
            }

            if (data.SellPrice != selection.UnitPrice)
            {
                message = "판매 가격이 변경되었습니다. 다시 선택해주세요.";
                return false;
            }

            long itemTotal = (long)data.SellPrice * selection.Count;

            // 합산 전에 소지금의 int 범위 검사
            if (itemTotal > (long)int.MaxValue - save.money - total)
            {
                message = "판매 후 소지금이 최대치를 초과합니다.";
                return false;
            }

            total += itemTotal;
        }

        bTrading = true;

        try
        {
            int previousMoney = save.money;

            // 인벤토리 변경 이벤트가 새 소지금을 읽도록 먼저 반영
            save.money += (int)total;

            if (!inventory.RemoveSaleSelections(transaction))
            {
                save.money = previousMoney;

                message = "아이템이 변경되었습니다. 다시 선택해주세요.";
                return false;
            }

            message = $"{total:N0} G에 판매했습니다.";

            AppendSaveResult(ref message);
            return true;
        }
        finally
        {
            bTrading = false;
        }
    }

    private void AppendSaveResult(ref string message)
    {
        if (!playerSave.TrySave(out string error))
        {
            message +=
                "\n거래는 완료됐지만 저장에 실패했습니다." +
                "\n게임을 종료하기 전에 다시 저장해주세요.";

            Debug.LogWarning(error, this);
        }
    }
}