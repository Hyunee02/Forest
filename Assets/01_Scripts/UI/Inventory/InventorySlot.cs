using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 중인 아이템
        GameObject dropped = eventData.pointerDrag;

        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

        if (draggableItem != null)
            draggableItem.parentBeforeDrag = transform;
    }
}
