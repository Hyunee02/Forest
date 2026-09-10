using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [Header("<< Item >>")]
    [SerializeField] private ItemData_SO itemData;

    [Header("<< Amount >>")]
    [SerializeField] private int amount = 1;

    private bool pickedUp;

    public ItemData_SO ItemData => itemData;
    public int Amount => amount;

    public bool Pickup(PlayerInventory inventory)
    {
        if (pickedUp)
            return false;

        if (inventory == null || itemData == null || amount <= 0)
        {
            Debug.LogWarning("WorldItem의 ItemData가 없습니다.");
            return false;
        }

        // 아이템 인벤에 추가할 수 있는지 검사
        if (!inventory.CanAddItem(ItemData.id, amount))
        {
            Debug.Log($"CanAddItem 실패 / ID : {itemData.id}, 수량 : {amount}", this);

            return false;
        }
            //return false;

        int durability = 0;

        if (itemData.itemType == ItemTypeSO.Tool)
        {
            ToolData_SO toolData =
                inventory.GetToolData(itemData.id);

            if (toolData != null)
                durability = toolData.durability;
        }

        bool success = inventory.AddItem(
            itemData.id,
            amount,
            durability
        );

        if (!success)
        {
            Debug.Log("AddItem 실패");
        }
            //return false;

        // 중복 줍기 방지
        pickedUp = true;
        gameObject.SetActive(false);
        Destroy(gameObject);

        return true;
    }
}