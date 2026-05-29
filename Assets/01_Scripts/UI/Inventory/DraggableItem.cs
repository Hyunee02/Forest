using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ItemInfo infoPrefab;

    private Image image;

    private ItemData itemData;

    public Transform parentBeforeDrag;

    public ItemData ItemData => itemData;

    public void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Init(ItemData data, Sprite sprite)
    {
        itemData = data;
        image.sprite = sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 전 아이템 원래 부모 저장
        parentBeforeDrag = transform.parent;

        // 아이템 부모 root으로 변경
        transform.SetParent(transform.root);

        // 아이템을 부모 안에서 제일 마지막 자식으로
        transform.SetAsLastSibling();

        // 이미지 raycast 꺼주기
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 아이템 부모 원래 부모로 변경
        transform.SetParent(parentBeforeDrag);

        // 이미지 위치 초기화
        image.rectTransform.localPosition = Vector3.zero;

        // 이미지 raycast 켜주기
        image.raycastTarget = true;
    }

    private ItemInfo itemInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemInfo = Instantiate(infoPrefab, transform, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(itemInfo);
    }
}
