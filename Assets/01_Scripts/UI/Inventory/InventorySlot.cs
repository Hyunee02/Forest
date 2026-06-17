using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIcon;

    private PlayerInventory inventory;

    public int Index {  get; private set; }

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="index"></param>
    public void Init(PlayerInventory inventory, int index)
    {
        this.inventory = inventory;
        Index = index;

        if (itemIcon != null)
            itemIcon.raycastTarget = false;
    }

    /// <summary>
    /// 슬롯 아이콘 바꾸기
    /// </summary>
    /// <param name="sprite"></param>
    public void SetIcon(Sprite sprite)
    {
        if (itemIcon == null)
            return;

        itemIcon.sprite = sprite;
        // Sprite가 null이 아니면 켜기, null이면 끄기
        itemIcon.enabled = sprite != null;
    }

    /// <summary>
    /// 슬롯에 있는 아이콘 Sprite 반환
    /// </summary>
    /// <returns></returns>
    public Sprite GetIcon()
    {
        if (itemIcon == null)
            return null;

        return itemIcon.sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        inventory.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        inventory.Drag(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        inventory.Drop(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        inventory.EndDrag();
    }
}
