using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [Header("<< Item >>")]
    [SerializeField] private ItemData_SO itemData;

    [Header("<< Amount >>")]
    [SerializeField] private int amount = 1;

    public ItemData_SO ItemData => itemData;
    public int Amount => amount;

    public bool Pickup(PlayerInventory inventory)
    {
        if (inventory == null)
            return false;

        if (itemData == null)
        {
            Debug.LogWarning("WorldItem의 ItemData가 없습니다.");
            return false;
        }

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
            return false;

        Destroy(gameObject);

        return true;
    }
}