using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot :
    MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("Display")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private CanvasGroup visuals;
    [SerializeField] private GameObject countRoot;

    private InventoryUI owner;
    
    public InventoryUI Owner => owner;
    public int SlotIndex { get; private set; }
    public Sprite IconSprite => icon != null ? icon.sprite : null;

    public void Init(InventoryUI inventoryUI, int index)
    {
        owner = inventoryUI;
        SlotIndex = index;
    }

    /// <summary>
    /// 슬롯 정보 갱신
    /// </summary>
    /// <param name="item"></param>
    /// <param name="data"></param>
    /// <param name="toolData"></param>
    /// <param name="bEquipped"></param>
    public void Refresh(InventoryItem item, ItemData_SO data, ToolData toolData, bool bEquipped)
    {
        bool hasItem = item != null
            && !item.BEmpty
            && data != null;

        if (visuals != null)
        {
            visuals.alpha = hasItem ? 1f : 0f;

            visuals.interactable = false;
            visuals.blocksRaycasts = false;
        }

        bool showCount = hasItem && data.ItemType != ItemTypeSO.Tool;

        if (countRoot != null)
            countRoot.SetActive(showCount);

        if (!hasItem)
            return;

        if (icon != null)
        {
            Sprite sprite = data.Icon;

            if (sprite == null && toolData != null)
                sprite = toolData.Icon;

            icon.sprite = sprite;
            icon.enabled = sprite != null;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        if (countText != null)
        {
            countText.text = item.count > 1
                ? item.count.ToString() : "";
        }
    }

    /// <summary>
    /// 클릭했을 때
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null || eventData.dragging)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            owner.ToggleEquip(SlotIndex);
        }
    }

    /// <summary>
    /// 드래그 시작
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (owner != null)
            owner.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (owner != null)
            owner.UpdateDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (owner != null)
            owner.CancelDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (owner == null || eventData.pointerDrag == null)
            return;

        InventorySlot slot = eventData.pointerDrag.GetComponent<InventorySlot>();

        owner.DropOn(SlotIndex, slot);
    }
}