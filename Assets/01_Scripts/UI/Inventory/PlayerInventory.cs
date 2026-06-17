using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("----- UI -----")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotParent;
    [SerializeField] private Image dragIcon;

    [Header("----- Option -----")]
    [SerializeField] private int maxItemCount = 20;

    private List<InventorySlot> slots;
    private List<string> itemIds;

    private PlayerBindInput input;

    private InventorySlot dragStartSlot;
    private bool bDrag;
    private bool bOpen;

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();

        InitSlots();
        InitItems();

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
            dragIcon.raycastTarget = false;
        }

        bOpen = false;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        UpdateAllSlots();
    }

    private void OnEnable()
    {
        input.OnInventoryInput += ToggleInventory;
    }

    private void OnDisable()
    {
        input.OnInventoryInput -= ToggleInventory;
    }

    private void Update()
    {
        
    }

    private void ToggleInventory()
    {
        bOpen = !bOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(bOpen);

        if (!bOpen)
            EndDrag();
    }

    private void InitSlots()
    {
        slots = new List<InventorySlot>();
        slots.Clear();

        // 비활성화 슬롯들까지 포함해서 찾기
        InventorySlot[] findSlots = slotParent.GetComponentsInChildren<InventorySlot>(true);

        for (int i = 0; i < findSlots.Length; i++)
        {
            if (i >= maxItemCount)
            {
                findSlots[i].gameObject.SetActive(false);
                continue;
            }

            findSlots[i].Init(this, i);
            slots.Add(findSlots[i]);
        }
    }

    private void InitItems()
    {
        itemIds = new List<string>();
        itemIds.Clear();

        for (int i = 0; i < slots.Count; i++)
        {
            itemIds.Add("");
        }
    }

    private bool AddItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return false;

        for (int i = 0; i < itemIds.Count; i++)
        {
            if (string.IsNullOrEmpty(itemIds[i]))
            {
                itemIds[i] = itemId;
                UpdateSlot(i);
                return true;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다.");
        return false;
    }

    public bool HasItem(string itemId)
    {
        for (int i = 0; i < itemIds.Count; i++)
        {
            if (itemIds[i] == itemId)
                return true;
        }

        return false;
    }

    public bool RemoveItem(string itemId)
    {
        for (int i = 0; i < itemIds.Count; i++)
        {
            if (itemIds[i] == itemId)
            {
                itemIds[i] = "";
                UpdateSlot(i);
                return true;
            }

            return false;
        }
    }

    public string GetItemId(int index)
    {
        if (index < 0 || index >= itemIds.Count)
            return "";

        return itemIds[index];
    }

    public void BeginDrag(InventorySlot slot, PointerEventData eventData)
    {
        if (!bOpen)
            return;

        string itemId = GetItemId(slot.Index);

        if (string.IsNullOrEmpty(itemId))
            return;

        dragStartSlot = slot;
        bDrag = true;

        if (dragIcon != null)
        {
            dragIcon.sprite = slot.GetICon();
            dragIcon.gameObject.SetActive(true);
            MoveDragIcon(eventData);
        }
    }

    public void Drag(PointerEventData evenData)
    {
        if (!bDrag)
            return;

        MoveDragIcon(evenData);
    }

    public void Drop(InventorySlot dropSlot)
    {
        if (!bDrag)
            return;

        if (dragStartSlot == null)
            return;

        SwapItem(dragStartSlot.Index, dropSlot.Index);
    }

    public void EndDrag()
    {
        if (dragIcon != null)
            dragIcon.gameObject.SetActive(false);

        dragStartSlot = null;
        bDrag = false;
    }

    private void SwapItem(int startIndex, int dropIndex)
    {
        if (startIndex == dropIndex)
            return;

        string temp = itemIds[startIndex];
        itemIds[startIndex] = itemIds[dropIndex];
        itemIds[dropIndex] = temp;

        UpdateSlot(startIndex);
        UpdateSlot(dropIndex);
    }

    private void UpdateAllSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            UpdateSlot(i);
        }
    }

    private void UpdateSlot(int index)
    {
        string itemId = GetItemId(index);

        if (string.IsNullOrEmpty(itemId))
        {
            slots[index].SetIcon(null);
            return;
        }

        Sprite sprite = ItemLoadManager.Instance.GetItemSprite(itemId);
        slots[index].SetIcon(sprite);
    }

    private void MoveDragIcon(PointerEventData eventData)
    {
        if (dragIcon == null)
            return;

        dragIcon.transform.position = eventData.position;
    }
} 
