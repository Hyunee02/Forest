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

        if (inventory == null)
            return false;

        if (itemData == null)
        {
            Debug.LogWarning("WorldItem의 ItemData가 없습니다.", this);
            return false;
        }

        //// 인벤토리에 넣을 수 있는지 먼저 확인
        //if (!inventory.CanAddItem(itemData.id, amount))
        //{
        //    Debug.Log("인벤토리에 공간이 없습니다.", this);
        //    return false;
        //}

        int durability = 0;

        // 도구라면 기본 내구도 가져오기
        if (itemData.itemType == ItemTypeSO.Tool)
        {
            ToolData toolData = inventory.GetToolData(itemData.id);

            if (toolData != null)
                durability = toolData.Durability;
        }

        // 인벤토리에 추가
        bool success = inventory.AddItem(
            itemData.id,
            amount,
            durability
        );

        if (!success)
        {
            Debug.Log("아이템 추가 실패", this);
            return false;
        }

        // 중복 획득 방지
        pickedUp = true;

        // 월드 아이템 제거
        Destroy(gameObject);

        return true;
    }
}