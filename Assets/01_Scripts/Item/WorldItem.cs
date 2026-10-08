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
        if (pickedUp || inventory == null || itemData == null)
            return false;

        bool success = inventory.AddItem(itemData.Id, amount);

        if (!success)
        {
            Debug.Log("아이템 추가 실패", this);
            return false;
        }

        // 중복 획득 방지
        pickedUp = true;
        gameObject.SetActive(false);
        // 월드 아이템 제거
        Destroy(gameObject);

        return true;
    }
}