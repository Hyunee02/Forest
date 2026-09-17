using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform parentAfterDrag;

    public int SlotIndex { get; private set; }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetSlotIndex(int index)
    {
        SlotIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 시작!!!");

        parentAfterDrag = transform.parent;

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            transform.SetParent(canvas.rootCanvas.transform, true);

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("드래그 끝!!!!");

        ReturnToSlot();

        canvasGroup.blocksRaycasts = true;
    }

    public void ReturnToSlot()
    {
        if (parentAfterDrag == null)
            return;

        transform.SetParent(parentAfterDrag, false);

        RectTransform rect = transform as RectTransform;

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;
    }
}