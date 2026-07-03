using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Slider durabilitySlider;

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

        itemIcon.raycastTarget = false;
        countText.raycastTarget = false;
        durabilitySlider.interactable = false;

        Clear();
    }

    /// <summary>
    /// 아이템 세팅
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="count"></param>
    /// <param name="toolData"></param>
    /// <param name="currentDurability"></param>
    public void SetItem(Sprite sprite, int count, ToolData toolData, int currentDurability)
    {
        itemIcon.sprite = sprite;
        itemIcon.gameObject.SetActive(sprite != null);

        countText.text = count > 1 ? count.ToString() : "";
        countText.gameObject.SetActive(count > 1);

        bool bTool = toolData != null;
        durabilitySlider.gameObject.SetActive(bTool);

        if (bTool)
        {
            durabilitySlider.maxValue = toolData.durability;
            durabilitySlider.value = currentDurability;
        }
    }

    /// <summary>
    /// 아이템 초기화
    /// </summary>
    public void Clear()
    {
        itemIcon.sprite = null;
        itemIcon.gameObject.SetActive(false);

        countText.text = "";
        countText.gameObject.SetActive(false);

        durabilitySlider.value = 0f;
        durabilitySlider.gameObject.SetActive(false);
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
        inventory.EndDrag(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        inventory.SelectSlot(this, eventData.position);
    }
}