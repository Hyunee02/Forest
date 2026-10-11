using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerShop : MonoBehaviour
{
    private PlayerInventory inventory;
    private PlayerEquip equip;

    private bool bTrading;

    public PlayerInventory Inventory => inventory;
    public bool IsUsingTool => equip != null && equip.BUse;

    public int Money
    {
        get
        {
            GameSaveData data = GameSession.Instance.CurrentData;
            return data != null ? data.money : 0;
        }
    }

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        equip = GetComponent<PlayerEquip>();
    }

    /// <summary>
    /// 아이템 개수 확인
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public int GetOwnedCount(string itemId)
    {
        int total = 0;

        for (int i = 0; i < inventory.SlotCount; i++)
        {
            InventoryItem item = inventory.GetItem(i);

            if (item == null || item.BEmpty)
                continue;

            if (item.itemId == itemId)
                total += item.count;
        }

        return total;
    }

    /// <summary>
    /// 에러 메세지
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private bool ConfigError(string detail, out string message)
    {
        Debug.LogError($"[PlayerShop] {detail}", this);
        message = "";
        return false;
    }

    /// <summary>
    /// 아이템 구매
    /// </summary>
    /// <param name="shop"></param>
    /// <param name="itemId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool TryBuy(ShopData shop, string itemId, out string message)
    {
        message = "";

        if (bTrading || IsUsingTool)
        {
            message = "지금은 구매할 수 없습니다.";
            return false;
        }

        if (shop == null)
            return ConfigError("ShopData가 없습니다.", out message);

        if (!shop.CanBuy(itemId))
        {
            message = "구매할 수 없는 상품입니다.";
            return false;
        }

        GameSaveData save = GameSession.Instance.CurrentData;
        ItemData_SO data = inventory.GetItemData(itemId);

        if (save == null)
            return ConfigError("GameSession 데이터가 없습니다.", out message);

        if (data == null || data.Buy < 0)
            return ConfigError($"상품 데이터 오류 : {itemId}", out message);

        if (save.money < 0)
            return ConfigError("소지금이 음수입니다.", out message);

        // CanAddItem 실패가 도구 데이터 오류인지 구분
        if (data.ItemType == ItemTypeSO.Tool)
        {
            ToolData tool = inventory.GetToolData(itemId);

            if (tool == null || tool.Durability <= 0)
                return ConfigError($"도구 데이터 오류 : {itemId}", out message);
        }

        if (save.money < data.Buy)
        {
            message = "소지금이 부족합니다.";
            return false;
        }

        if (!inventory.CanAddItem(itemId, 1))
        {
            message = "인벤토리 공간이 부족합니다.";
            return false;
        }

        bTrading = true;

        try
        {
            int previousMoney = save.money;

            // 인벤토리 이벤트에서 변경된 소지금 먼저 볼 수 있게 먼저 반영
            save.money -= data.Buy;

            if (!inventory.AddItem(itemId, 1))
            {
                save.money = previousMoney;
                message = "구매하지 못했습니다.\n다시 시도해주세요.";
                return false;
            }

            message = $"{data.ItemName} 1개를 구매했습니다.";
            return true;
        }
        
        finally
        {
            bTrading = false;
        }
    }

    /// <summary>
    /// 아이템 판매 (상점 종류와 무관)
    /// </summary>
    /// <param name="shop"></param>
    /// <param name="slotIndex"></param>
    /// <param name="expectedItem"></param>
    /// <param name="expectedItemId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool TrySell(ShopData shop, int slotIndex, InventoryItem expectedItem, string expectedItemId, out string message)
    {
        message = "";

        if (bTrading || IsUsingTool)
        {
            message = "지금은 거래할 수 없습니다.";
            return false;
        }

        GameSaveData save = GameSession.Instance.CurrentData;

        if (save == null)
            return ConfigError("GameSession 데이터가 없습니다.", out message);

        InventoryItem item = inventory.GetItem(slotIndex);

        // 선택 이후에 슬롯 바뀌었으면 거래 취소
        if (item == null
            || item.BEmpty
            || !ReferenceEquals(item, expectedItem)
            || item.itemId != expectedItemId)
        {
            message = "아이템이 변경되었습니다\n다시 선택해주세요.";
            return false;
        }

        ItemData_SO data = inventory.GetItemData(item.itemId);

        if (data == null)
            return ConfigError($"아이템 데이터 없음 : {item.itemId}", out message);

        if (data.Sell < 0)
        {
            message = "판매할 수 없는 아이템입니다.";
            return false;
        }

        if (save.money < 0)
            return ConfigError("소지금이 음수입니다.", out message);

        if (save.money > int.MaxValue - data.Sell)
        {
            message = "소지금이 최대치에 도달했습니다.";
            return false;
        }

        bTrading = true;

        try
        {
            int previousMoney = save.money;
            save.money += data.Sell;

            if (!inventory.RemoveItem(slotIndex, 1))
            {
                save.money = previousMoney;
                message = "판매하지 못했습니다.\n다시 선택해주세요.";
                return false;
            }

            message = $"{data.ItemName} 1개를 판매했습니다.";
            return true;
        }

        finally
        {
            bTrading = false;
        }
    }
}
