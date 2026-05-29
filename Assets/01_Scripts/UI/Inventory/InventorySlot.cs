using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private ItemInfo infoPrefab;
    [SerializeField] private GameObject itemPrefab;

    private GameObject item;

    ItemData itemData;

    public ItemData ItemData => itemData;

    public void Init(ItemData data)
    {
        itemData = data;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        item = Instantiate(itemPrefab, transform, false);
        Image itemImage = item.GetComponent<Image>();
        Sprite sprite = ItemLoadManager.Instance.Load<Sprite>($"Prefab/{itemData.imagePath}");
        itemImage.sprite = sprite;
    }

    public void OnDrag(PointerEventData eventData)
    {
        item.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 1개의 슬롯에는 1개의 아이템만
        if (transform.childCount > 0)
            return;

        // 드랍된 아이템
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

        if (draggableItem != null)
            draggableItem.parentBeforeDrag = transform;
    }
}
