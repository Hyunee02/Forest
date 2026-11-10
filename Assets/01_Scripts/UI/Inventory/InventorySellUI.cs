using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySellUI : MonoBehaviour
{
    [Header("인벤토리")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("판매 패널")]
    [SerializeField] private GameObject saleControls;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;

    [Header("현재 슬롯 수량 조절")]
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private TMP_Text quantityText;

    [Header("표시")]
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text messageText;

    private PlayerShop shop;
    private PlayerMove move;

    private ShopData currentShop;

    private readonly Dictionary<int, SaleSelection> selected = new();

    private int editingIndex = -1;
    private int openedDay;

    public bool BOpen { get; private set; }

    public bool CanOpen => isActiveAndEnabled
        && !BOpen
        && inventoryUI != null
        && inventoryUI.CanOpenSales
        && saleControls != null
        && sellButton != null
        && closeButton != null
        && minusButton != null
        && plusButton != null
        && removeButton != null
        && quantityText != null
        && totalText != null
        && moneyText != null
        && messageText != null
        && BindPlayer()
        && shop.IsReady
        && inventoryUI.Inventory == shop.Inventory
        && !shop.IsUsingTool;

    private void Awake()
    {
        if (saleControls != null)
            saleControls.SetActive(false);
    }

    private void OnEnable()
    {
        sellButton.onClick.AddListener(SellSelected);
        closeButton.onClick.AddListener(Close);

        minusButton.onClick.AddListener(Decrease);
        plusButton.onClick.AddListener(Increase);
        removeButton.onClick.AddListener(RemoveEditingSelection);
    }

    private void OnDisable()
    {
        sellButton.onClick.RemoveListener(SellSelected);
        closeButton.onClick.RemoveListener(Close);

        minusButton.onClick.RemoveListener(Decrease);
        plusButton.onClick.RemoveListener(Increase);
        removeButton.onClick.RemoveListener(RemoveEditingSelection);

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

        if (inventoryUI == null || !inventoryUI.BOpen)
        {
            Close();
            return;
        }

        if (openedDay != shop.Day)
        {
            openedDay = shop.Day;
            ClearSelection();

            messageText.text =
                "날짜가 변경되었습니다. 판매할 아이템을 다시 선택해주세요.";
        }

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

    public void Open(ShopData data)
    {
        if (data == null || !CanOpen)
            return;

        if (!data.Validate(out string error))
        {
            Debug.LogError($"[InventorySellUI] {error}", this);
            return;
        }

        currentShop = data;
        openedDay = shop.Day;

        ClearSelection();

        inventoryUI.SetSellMode(true);
        inventoryUI.SetOpen(true);

        if (!inventoryUI.BOpen)
        {
            inventoryUI.SetSellMode(false);
            currentShop = null;
            return;
        }

        BOpen = true;

        inventoryUI.OnSaleSlotClicked += SelectSlot;
        inventoryUI.OnClosed += Close;

        shop.Inventory.OnChanged += OnInventoryChanged;

        move.SetMoveEnabled(false);

        saleControls.SetActive(true);

        messageText.text = "판매할 슬롯을 선택해주세요.";

        RefreshSummary();
    }

    public void Close()
    {
        if (!BOpen)
            return;

        BOpen = false;

        inventoryUI.OnSaleSlotClicked -= SelectSlot;
        inventoryUI.OnClosed -= Close;

        if (shop != null && shop.Inventory != null)
            shop.Inventory.OnChanged -= OnInventoryChanged;

        ClearSelection();

        inventoryUI.SetSellMode(false);
        inventoryUI.SetOpen(false);

        saleControls.SetActive(false);

        if (move != null)
            move.SetMoveEnabled(true);

        currentShop = null;
    }

    private void SelectSlot(int index)
    {
        if (!BOpen)
            return;

        InventoryItem item = shop.Inventory.GetItem(index);

        if (item == null || item.BEmpty)
            return;

        ItemData_SO data =
            shop.Inventory.GetItemData(item.itemId);

        if (data == null)
            return;

        if (!currentShop.CanSell(item.itemId) || data.Sell <= 0)
        {
            messageText.text =
                $"{data.ItemName}은(는) 이 상점에서 판매할 수 없습니다.";

            return;
        }

        // 이미 선택한 슬롯이면 편집 대상만 변경
        if (selected.ContainsKey(index))
        {
            editingIndex = index;
            messageText.text = "";
            RefreshSummary();
            return;
        }

        if (GetMaximumForSlot(index) <= 0)
        {
            messageText.text =
                "오늘 판매 가능한 수량을 모두 선택했습니다.";

            return;
        }

        selected[index] = new SaleSelection(
            index,
            item,
            1,
            data.Sell);

        editingIndex = index;

        inventoryUI.SetSaleHightlight(index, true);

        messageText.text = "";

        RefreshSummary();
    }

    // 다른 슬롯에서 선택한 같은 아이템 수량을 제외한 최대값
    private int GetMaximumForSlot(int index)
    {
        InventoryItem item = shop.Inventory.GetItem(index);

        if (item == null || item.BEmpty || currentShop == null)
            return 0;

        long otherSelected = 0;

        foreach (KeyValuePair<int, SaleSelection> pair in selected)
        {
            if (pair.Key != index &&
                pair.Value.ItemId == item.itemId)
            {
                otherSelected += pair.Value.Count;
            }
        }

        long available =
            (long)shop.GetSellRemaining(currentShop, item.itemId)
            - otherSelected;

        if (available <= 0)
            return 0;

        return (int)System.Math.Min(item.count, available);
    }

    private void Decrease()
    {
        ChangeAmount(-1);
    }

    private void Increase()
    {
        ChangeAmount(1);
    }

    private void ChangeAmount(int delta)
    {
        if (!selected.TryGetValue(
                editingIndex,
                out SaleSelection previous))
        {
            return;
        }

        int maximum = GetMaximumForSlot(editingIndex);

        long requested = (long)previous.Count + delta;

        if (requested < 1 || requested > maximum)
            return;

        InventoryItem item =
            shop.Inventory.GetItem(editingIndex);

        selected[editingIndex] = new SaleSelection(
            editingIndex,
            item,
            (int)requested,
            previous.UnitPrice);

        RefreshSummary();
    }

    private void RemoveEditingSelection()
    {
        if (editingIndex < 0)
            return;

        selected.Remove(editingIndex);
        inventoryUI.SetSaleHightlight(editingIndex, false);

        editingIndex = -1;

        RefreshSummary();
    }

    private void ClearSelection()
    {
        selected.Clear();
        editingIndex = -1;

        if (inventoryUI != null)
            inventoryUI.ClearSaleHighlights();
    }

    private void OnInventoryChanged()
    {
        ClearSelection();

        messageText.text =
            "인벤토리가 변경되어 선택을 초기화했습니다.";

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        if (!BOpen || currentShop == null)
            return;

        long total = 0;
        bool valid = selected.Count > 0;

        StringBuilder summary = new();

        foreach (KeyValuePair<int, SaleSelection> pair in selected)
        {
            SaleSelection selection = pair.Value;

            ItemData_SO data =
                shop.Inventory.GetItemData(selection.ItemId);

            if (data == null ||
                !currentShop.CanSell(selection.ItemId) ||
                data.Sell != selection.UnitPrice ||
                selection.Count > GetMaximumForSlot(pair.Key))
            {
                valid = false;
            }

            summary.AppendLine(
                $"{data?.ItemName ?? selection.ItemId} × {selection.Count:N0}");

            // 최대 소지금을 넘은 뒤에는 합계를 더 키울 필요가 없음
            if (total <= int.MaxValue)
                total += selection.Total;
        }

        if (!shop.Inventory.ValidateSaleSelections(
                new List<SaleSelection>(selected.Values)))
        {
            valid = false;
        }

        if (total > (long)int.MaxValue - shop.Money)
            valid = false;

        if (total > int.MaxValue)
            summary.Append("판매 금액이 최대치를 초과합니다.");
        else
            summary.Append($"판매 총액 {total:N0} G");

        totalText.text = summary.ToString();
        moneyText.text = $"소지금 {shop.Money:N0} G";

        sellButton.interactable =
            valid &&
            total > 0 &&
            shop.IsReady &&
            !shop.IsUsingTool;

        RefreshQuantityEditor();
    }

    private void RefreshQuantityEditor()
    {
        bool hasSelection =
            selected.TryGetValue(
                editingIndex,
                out SaleSelection selection);

        minusButton.interactable =
            hasSelection && selection.Count > 1;

        plusButton.interactable =
            hasSelection &&
            selection.Count < GetMaximumForSlot(editingIndex);

        removeButton.interactable = hasSelection;

        if (!hasSelection)
        {
            quantityText.text = "수량을 바꿀 슬롯을 선택해주세요.";
            return;
        }

        ItemData_SO data =
            shop.Inventory.GetItemData(selection.ItemId);

        ShopItemEntry entry =
            currentShop.GetSellEntry(selection.ItemId);

        int remaining =
            shop.GetSellRemaining(currentShop, selection.ItemId);

        string limit = entry != null && entry.dailyLimit < 0
            ? "하루 제한 없음"
            : $"오늘 남은 판매 한도 {remaining:N0}개";

        quantityText.text =
            $"{data?.ItemName ?? selection.ItemId}\n" +
            $"판매 수량 {selection.Count:N0} / 보유 {selection.SourceCount:N0}\n" +
            limit;
    }

    private void SellSelected()
    {
        if (!BOpen || selected.Count == 0)
            return;

        List<SaleSelection> transaction =
            new(selected.Values);

        shop.TrySellSelected(
            currentShop,
            transaction,
            out string message);

        ClearSelection();

        inventoryUI.RefreshAll();
        RefreshSummary();

        // 거래 중 발생한 인벤토리 이벤트 안내보다 결과를 나중에 표시
        messageText.text = message;
    }
}