using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private Image image;

    private Transform parentAfterDrag;

    public int SlotIndex { get; private set; }

    public void SetSlotIndex(int index)
    {
        SlotIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 시작!!!");

        parentAfterDrag = transform.parent;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 끝!!!!");

        transform.SetParent(parentAfterDrag);

        RectTransform rectTransform =
            transform as RectTransform;

        if (rectTransform != null)
            rectTransform.anchoredPosition = Vector2.zero;

        image.raycastTarget = true;
    }
}