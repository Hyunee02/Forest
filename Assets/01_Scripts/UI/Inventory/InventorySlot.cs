using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot :
    MonoBehaviour,
    IDropHandler
{
    [SerializeField] private int slotIndex;

    public int SlotIndex => slotIndex;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        DraggableItem draggableItem =
            eventData.pointerDrag
                .GetComponent<DraggableItem>();

        if (draggableItem == null)
            return;

        PlayerInventory inventory =
            FindFirstObjectByType<PlayerInventory>();

        if (inventory == null)
            return;

        inventory.MoveItem(
            draggableItem.SlotIndex,
            slotIndex
        );
    }
}