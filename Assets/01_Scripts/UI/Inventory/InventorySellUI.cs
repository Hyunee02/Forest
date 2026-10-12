using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySellUI : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Sale Controls")]
    [SerializeField] private GameObject saleControls;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;

    [Header("Text")]
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text messageText;

    private PlayerShop shop;
    private PlayerMove move;

    private readonly Dictionary<int, SaleSelection> selected = new();

    public bool BOpen { get; private set; }

    public bool CanOpen => isActiveAndEnabled
        && !BOpen
        && inventoryUI != null
        && inventoryUI.CanOpenSales
        && saleControls != null
        && sellButton != null
        && closeButton != null
        && totalText != null
        && moneyText != null
        && messageText != null
        && BindPlayer()
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
    }

    private void OnDisable()
    {
        sellButton.onClick.RemoveListener(SellSelected);
        closeButton.onClick.RemoveListener(Close);

        Close();
    }

    private void Update()
    {
        if (!BOpen)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
    }

    private void LateUpdate()
    {
        if (!BOpen)
            return;

        // 다른 경로로 인벤토리가 닫힌 경우에도 상태 정리
        if (inventoryUI == null || !inventoryUI.BOpen)
        {
            Close();
            return;
        }

        // 다른 기능에서 소지금이 변경되는 경우도 반영
        RefreshSummary();
    }

    private bool BindPlayer()
    {
        if (shop != null && move != null)
            return true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return false;

        shop = player.GetComponent<PlayerShop>();
        move = player.GetComponent<PlayerMove>();

        return shop != null && move != null;
    }

    public void Open()
    {
        if (!CanOpen)
            return;

        selected.Clear();

        inventoryUI.SetSellMode(true);
        inventoryUI.SetOpen(true);

        if (!inventoryUI.BOpen)
        {
            inventoryUI.SetSellMode(false);
            return;
        }

        BOpen = true;

        inventoryUI.OnSaleSlotClicked += ToggleSelection;
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

        if (inventoryUI != null)
        {
            inventoryUI.OnSaleSlotClicked -= ToggleSelection;
            inventoryUI.OnClosed -= Close;
        }

        if (shop != null && shop.Inventory != null)
            shop.Inventory.OnChanged -= OnInventoryChanged;

        selected.Clear();

        if (inventoryUI != null)
        {
            inventoryUI.SetSellMode(false);
            inventoryUI.SetOpen(false);
        }

        if (saleControls != null)
            saleControls.SetActive(false);

        if (move != null)
            move.SetMoveEnabled(true);
    }

    private void ToggleSelection(int index)
    {
        if (!BOpen)
            return;

        // 이미 선택된 슬롯이면 선택 해제
        if (selected.Remove(index))
        {
            inventoryUI.SetSaleHightlight(index, false);

            messageText.text = "";

            RefreshSummary();

            return;
        }

        InventoryItem item = shop.Inventory.GetItem(index);

        if (item == null || item.BEmpty || item.count <= 0)
            return;

        ItemData_SO data = shop.Inventory.GetItemData(item.itemId);

        if (data == null)
        {
            Debug.LogError($"아이템 데이터가 없습니다 : {item.itemId}", this);

            return;
        }

        if (data.Sell <= 0)
        {
            messageText.text = "판매할 수 없는 아이템입니다.";
            return;
        }

        SaleSelection selection = new SaleSelection(index, item, data.Sell);

        selected.Add(index, selection);

        inventoryUI.SetSaleHightlight(index, true);

        messageText.text = $"{data.ItemName} {item.count}개 선택";

        RefreshSummary();
    }

    private void OnInventoryChanged()
    {
        // 선택 이후 획득, 소모 등으로 내용이 바뀌면 다시 선택
        selected.Clear();

        inventoryUI.ClearSaleHighlights();

        messageText.text =
            "인벤토리가 변경되어 선택을 초기화했습니다.";

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        int total = 0;

        foreach (SaleSelection selection in selected.Values)
            total += selection.Total;

        int money = shop.Money;

        totalText.text = $"선택 {selected.Count}칸\n판매 총액 {total:N0} G";

        moneyText.text = $"소지금 {money:N0} G";

        sellButton.interactable =
            BOpen &&
            selected.Count > 0 &&
            total > 0 &&
            money >= 0 &&
            total <= (long)int.MaxValue - money &&
            !shop.IsUsingTool;
    }

    private void SellSelected()
    {
        if (!BOpen || selected.Count == 0)
            return;

        List<SaleSelection> transaction =
            new List<SaleSelection>(selected.Values);

        shop.TrySellSelected(
            transaction,
            out string message);

        // 성공 시에는 인벤토리 이벤트에서도 초기화
        // 실패한 경우도 다시 선택하도록 정리
        selected.Clear();

        inventoryUI.ClearSaleHighlights();
        inventoryUI.RefreshAll();

        RefreshSummary();

        // 인벤토리 이벤트 안내 문구 다음에 판매 결과 표시
        messageText.text = message;
    }

}
