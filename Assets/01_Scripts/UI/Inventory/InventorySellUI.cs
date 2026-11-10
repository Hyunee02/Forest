using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySellUI : MonoBehaviour
{
    [Header("판매 화면 전체")]
    [SerializeField] private GameObject sellPanel;

    [Header("왼쪽: 보유 물품")]
    [SerializeField] private Transform ownedSlotRoot;
    [SerializeField] private SellSlot ownedSlotPrefab;

    [Header("오른쪽: 판매할 물품")]
    [SerializeField] private Transform selectedSlotRoot;
    [SerializeField] private SellSlot selectedSlotPrefab;

    [Header("버튼")]
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;

    [Header("표시")]
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text messageText;

    [Header("선택 사항: 기존 인벤토리가 열려 있으면 닫기")]
    [SerializeField] private InventoryUI normalInventoryUI;

    private PlayerShop shop;
    private PlayerMove move;

    // 실제 인벤토리 슬롯 번호 → 판매 선택 정보
    private readonly Dictionary<int, SaleSelection> selected = new();

    // 생성한 UI 슬롯을 재사용
    private readonly List<SellSlot> ownedSlots = new();
    private readonly List<SellSlot> selectedSlots = new();

    public bool BOpen { get; private set; }

    public bool CanOpen =>
        isActiveAndEnabled &&
        !BOpen &&
        sellPanel != null &&
        sellPanel != gameObject &&
        !transform.IsChildOf(sellPanel.transform) &&
        ownedSlotRoot != null &&
        ownedSlotPrefab != null &&
        selectedSlotRoot != null &&
        selectedSlotPrefab != null &&
        sellButton != null &&
        closeButton != null &&
        totalText != null &&
        moneyText != null &&
        BindPlayer() &&
        shop.Inventory != null &&
        shop.IsReady &&
        !shop.IsUsingTool;

    private void Awake()
    {
        if (sellPanel != null)
            sellPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (sellButton != null)
            sellButton.onClick.AddListener(SellSelected);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (sellButton != null)
            sellButton.onClick.RemoveListener(SellSelected);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        Close();
    }

    private void Update()
    {
        if (BOpen &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void LateUpdate()
    {
        if (!BOpen)
            return;

        // 외부에서 소지금이나 거래 가능 상태가 바뀌어도 반영
        RefreshSummary();
    }

    private bool BindPlayer()
    {
        if (shop != null && move != null)
            return true;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return false;

        shop = player.GetComponent<PlayerShop>();
        move = player.GetComponent<PlayerMove>();

        return shop != null && move != null;
    }

    public void Open()
    {
        if (!CanOpen)
        {
            Debug.LogWarning(
                "[InventorySellUI] UI 연결, 활성 상태, " +
                "PlayerSave 준비 상태를 확인해주세요.",
                this);

            return;
        }

        // 일반 인벤토리 창은 사용하지 않음
        if (normalInventoryUI != null)
            normalInventoryUI.SetOpen(false);

        selected.Clear();

        BOpen = true;

        shop.Inventory.OnChanged += OnInventoryChanged;

        move.SetMoveEnabled(false);
        sellPanel.SetActive(true);

        ShowMessage("왼쪽에서 팔 물건을 선택해주세요.");
        RefreshView();
    }

    public void Close()
    {
        if (!BOpen)
            return;

        BOpen = false;

        if (shop != null && shop.Inventory != null)
            shop.Inventory.OnChanged -= OnInventoryChanged;

        selected.Clear();

        if (sellPanel != null)
            sellPanel.SetActive(false);

        if (move != null)
            move.SetMoveEnabled(true);
    }

    private void ChangeSelectionAmount(
        int inventoryIndex,
        int delta)
    {
        if (!BOpen || !shop.IsReady || shop.IsUsingTool)
            return;

        InventoryItem item =
            shop.Inventory.GetItem(inventoryIndex);

        if (item == null || item.BEmpty)
            return;

        ItemData_SO data =
            shop.Inventory.GetItemData(item.itemId);

        if (data == null || data.SellPrice < 0)
        {
            ShowMessage("판매 정보를 확인할 수 없습니다.");
            return;
        }

        int currentCount =
            selected.TryGetValue(
                inventoryIndex,
                out SaleSelection previous)
                    ? previous.Count
                    : 0;

        long requested = (long)currentCount + delta;

        int nextCount = (int)System.Math.Max(
            0L,
            System.Math.Min(requested, (long)item.count));

        if (nextCount == currentCount)
            return;

        if (nextCount == 0)
        {
            selected.Remove(inventoryIndex);
        }
        else
        {
            selected[inventoryIndex] = new SaleSelection(
                inventoryIndex,
                item,
                nextCount,
                data.SellPrice);
        }

        ShowMessage("");
        RefreshView();
    }

    private void OnInventoryChanged()
    {
        // 선택 후 다른 경로에서 아이템이 바뀌면 선택 초기화
        selected.Clear();

        ShowMessage("인벤토리가 변경되어 선택을 초기화했습니다.");
        RefreshView();
    }

    private void RefreshView()
    {
        if (!BOpen)
            return;

        RefreshOwnedSlots();
        RefreshSelectedSlots();
        RefreshSummary();
    }

    private void RefreshOwnedSlots()
    {
        int visibleCount = 0;

        for (int i = 0; i < shop.Inventory.SlotCount; i++)
        {
            InventoryItem item = shop.Inventory.GetItem(i);

            if (item == null || item.BEmpty)
                continue;

            ItemData_SO data =
                shop.Inventory.GetItemData(item.itemId);

            if (data == null)
                continue;

            SellSlot slot = GetSlot(
                ownedSlots,
                ownedSlotPrefab,
                ownedSlotRoot,
                visibleCount);

            int inventoryIndex = i;

            int selectedAmount =
                selected.TryGetValue(
                    inventoryIndex,
                    out SaleSelection selection)
                        ? selection.Count
                        : 0;

            slot.Bind(
                data,
                GetIcon(data),
                item.count,
                selectedAmount,
                false,

                // 슬롯 자체 클릭 시 판매 수량 +1
                () => ChangeSelectionAmount(inventoryIndex, 1),

                // +/- 버튼 클릭 시 수량 변경
                delta => ChangeSelectionAmount(inventoryIndex, delta)
            );

            visibleCount++;
        }

        HideUnusedSlots(ownedSlots, visibleCount);
    }

    private void RefreshSelectedSlots()
    {
        int visibleCount = 0;

        for (int i = 0; i < shop.Inventory.SlotCount; i++)
        {
            if (!selected.TryGetValue(
                    i,
                    out SaleSelection selection))
            {
                continue;
            }

            ItemData_SO data =
                shop.Inventory.GetItemData(selection.ItemId);

            if (data == null)
                continue;

            SellSlot slot = GetSlot(
                selectedSlots,
                selectedSlotPrefab,
                selectedSlotRoot,
                visibleCount);

            slot.Bind(
                data,
                GetIcon(data),
                selection.SourceCount,
                selection.Count,
                true,
                null,
                null
            );

            visibleCount++;
        }

        HideUnusedSlots(selectedSlots, visibleCount);
    }

    private SellSlot GetSlot(
        List<SellSlot> slots,
        SellSlot prefab,
        Transform root,
        int index)
    {
        while (slots.Count <= index)
        {
            SellSlot slot = Instantiate(prefab, root, false);
            slots.Add(slot);
        }

        SellSlot result = slots[index];

        result.gameObject.SetActive(true);
        return result;
    }

    private void HideUnusedSlots(
        List<SellSlot> slots,
        int visibleCount)
    {
        for (int i = visibleCount; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);
    }

    private Sprite GetIcon(ItemData_SO data)
    {
        if (data.Icon != null)
            return data.Icon;

        if (data.ItemType == ItemTypeSO.Tool)
            return shop.Inventory.GetToolData(data.Id)?.Icon;

        return null;
    }

    private void RefreshSummary()
    {
        decimal total = 0;
        long totalCount = 0;

        bool valid = selected.Count > 0;

        foreach (SaleSelection selection in selected.Values)
        {
            ItemData_SO data =
                shop.Inventory.GetItemData(selection.ItemId);

            if (data == null ||
                data.SellPrice < 0 ||
                data.SellPrice != selection.UnitPrice)
            {
                valid = false;
            }

            total +=
                (decimal)selection.UnitPrice * selection.Count;

            totalCount += selection.Count;
        }

        if (valid)
        {
            List<SaleSelection> selections = new(selected.Values);

            valid = shop.Inventory.ValidateSaleSelections(selections);
        }

        bool exceedsMoneyLimit =
            total > (decimal)int.MaxValue - shop.Money;

        if (exceedsMoneyLimit)
            valid = false;

        totalText.text = $"{total:N0} G";

        if (exceedsMoneyLimit)
            totalText.text += "\n판매 후 소지금이 최대치를 초과합니다.";

        moneyText.text = $"{shop.Money:N0} G";

        sellButton.interactable =
            valid &&
            shop.IsReady &&
            !shop.IsUsingTool;
    }

    private void SellSelected()
    {
        if (!BOpen || selected.Count == 0)
            return;

        // 거래 중 인벤토리 이벤트가 선택을 지우므로 복사
        List<SaleSelection> transaction = new(selected.Values);

        bool success = shop.TrySellSelected(
            transaction,
            out string message);

        if (success)
            selected.Clear();

        RefreshView();

        // 인벤토리 변경 안내보다 거래 결과를 나중에 표시
        ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}