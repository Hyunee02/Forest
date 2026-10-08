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
    /// 아이템 장착 확인
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool IsEquipped(InventoryItem item)
    {
        return equip != null && equip.IsEquipped(item);
    }

    public bool TryBuy(ShopData shop, string itemId, out string message)
    {
        message = "";

        if (bTrading || IsUsingTool)
        {
            message = "지금은 거래할 수 없습니다.";
            return false;
        }

        if (shop == null || !shop.CanBuy(itemId))
        {
            message = "이 상점에서 구매할 수 없는 상품입니다.";
            return false;
        }

        GameSaveData save = GameSession.Instance.CurrentData;
        ItemData_SO data = inventory.GetItemData(itemId);

        if (save == null)
        {
            Debug.Log("플레이어 데이터가 없습니다.");
            message = string.Empty;
            return false;
        }

        if (data == null || data.Buy < 0)
        {
            Debug.Log("상품 데이터가 올바르지 않습니다.");
            message = string.Empty;
            return false;
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

            save.money -= data.Buy;
        }
    }
}
