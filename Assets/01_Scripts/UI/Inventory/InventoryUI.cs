using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    [Header("Slots")]
    [SerializeField] private Transform slotRoot;
    [SerializeField] private InventorySlot slotPrefab;

    [Header("Drag")]
    [SerializeField] private Image dragIcon;

    [Header("Player")]
    private PlayerInventory inventory;
    private PlayerEquip equip;
    private PlayerBindInput input;

    private readonly List<InventorySlot> slots = new List<InventorySlot>();

    private int dragIndex = -1;
    private InventoryItem dragItem;

    public bool BOpen => inventoryPanel != null
        && inventoryPanel.activeInHierarchy;

    private void Awake()
    {
        GameObject player = GameObject.Find("Player");
        inventory = player.GetComponent<PlayerInventory>();
        equip = player.GetComponent<PlayerEquip>();
        input = player.GetComponent<PlayerBindInput>();

        if (inventory == null
            || inventoryPanel == null
            || slotRoot == null
            || slotPrefab == null
            || dragIcon == null)
        {
            enabled = false;
            return;
        }

        if (input == null || equip == null)
        {
            enabled = false;
            return;
        }

        dragIcon.raycastTarget = false;
        // 이미지 비율
        dragIcon.preserveAspect = true;
        dragIcon.gameObject.SetActive(false);

        equip.BindInventoryUI(this);
        inventoryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        inventory.OnChanged += RefreshAll;
        equip.OnEquipChanged += RefreshAll;
        input.OnInventoryInput += ToggleInventory;

        RefreshAll();
    }

    private void OnDisable()
    {
        inventory.OnChanged -= RefreshAll;
        equip.OnEquipChanged -= RefreshAll;
        input.OnInventoryInput -= ToggleInventory;

        CancelDrag();

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    #region > 버튼 연결
    public void UpgradeInventory()
    {
        inventory.Upgrade();
    }
    #endregion

    #region > 인벤토리 토글
    /// <summary>
    /// 인벤토리 On / Off
    /// </summary>
    public void ToggleInventory()
    {
        SetOpen(!BOpen);
    }

    /// <summary>
    /// 인벤토리 열기
    /// </summary>
    /// <param name="open"></param>
    public void SetOpen(bool open)
    {
        if (!isActiveAndEnabled)
            return;

        if (open && equip.BUse)
            return;

        CancelDrag();
        inventoryPanel.SetActive(open);

        if (open)
            RefreshAll();
    }
    #endregion

    #region > 드래그
    /// <summary>
    /// 드래그 시작
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="eventData"></param>
    public void BeginDrag(InventorySlot slot, PointerEventData eventData)
    {
        if (!BOpen || equip.BUse)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        InventoryItem item = inventory.GetItem(slot.SlotIndex);

        if (item == null || item.BEmpty)
            return;

        CancelDrag();

        dragIndex = slot.SlotIndex;
        dragItem = item;

        dragIcon.sprite = slot.IconSprite;
        dragIcon.gameObject.SetActive(true);
        dragIcon.transform.SetAsLastSibling();

        UpdateDrag(eventData);
    }

    /// <summary>
    /// 드래그 마우스 따라가기
    /// </summary>
    /// <param name="eventData"></param>
    public void UpdateDrag(PointerEventData eventData)
    {
        if (dragIndex < 0)
            return;

        dragIcon.rectTransform.position = eventData.position;
    }

    /// <summary>
    /// 다른 슬롯에 놓기
    /// </summary>
    /// <param name="targetIndex">놓은 인덱스</param>
    /// <param name="slot">어디에서 드래그 해왔는지</param>
    public void DropOn(int targetIndex, InventorySlot slot)
    {
        if (!BOpen
            || slot == null
            || slot.Owner != this
            || slot.SlotIndex != dragIndex
            || dragIndex < 0)
            return;

        int fromIndex = dragIndex;

        bool sameItem = ReferenceEquals(inventory.GetItem(fromIndex), dragItem);

        CancelDrag();

        if (!sameItem || equip.BUse)
            return;

        inventory.MoveItem(fromIndex, targetIndex);
    }
    #endregion


    /// <summary>
    /// 드래그 상태 초기화
    /// </summary>
    public void CancelDrag()
    {
        dragIndex = -1;
        dragItem = null;

        if (dragIcon != null)
            dragIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// 누락된 슬롯 생성
    /// </summary>
    private void CreateMissingSlots()
    {
        while (slots.Count < inventory.SlotCount)
        {
            int index = slots.Count;

            InventorySlot slot = Instantiate(slotPrefab, slotRoot, false);

            slot.name = $"Slot_{index}";
            slot.gameObject.SetActive(true);
            slot.Init(this, index);

            slots.Add(slot);
        }
    }

    /// <summary>
    /// 모든 슬롯 갱신
    /// </summary>
    public void RefreshAll()
    {
        CancelDrag();
        CreateMissingSlots();

        for (int i = 0; i < slots.Count; i++)
        {
            InventoryItem item = inventory.GetItem(i);

            ItemData_SO data =
                item != null && !item.BEmpty
                ? inventory.GetItemData(item.itemId) : null;

            ToolData toolData =
                data != null && data.itemType == ItemTypeSO.Tool
                ? inventory.GetToolData(item.itemId) : null;

            slots[i].Refresh(item, data, toolData, equip.IsEquipped(item));
        }
    }

    /// <summary>
    /// PlayerEquip한테 장착 전달
    /// </summary>
    /// <param name="slotIndex"></param>
    public void ToggleEquip(int slotIndex)
    {
        if (!BOpen || equip.BUse)
            return;

        equip.ToggleEquip(slotIndex);
    }
}