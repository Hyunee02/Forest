using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeButton;

    [Header("상품 목록")]
    [SerializeField] private Transform slotRoot;
    [SerializeField] private ShopSlot slotPrefab;

    [Header("일괄 구매")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text totalPriceText;

    [Header("표시")]
    [SerializeField] private TMP_Text moneyText;

    [Header("인벤토리")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("선택사항")]
    [SerializeField] private TMP_Text messageText;

    private readonly List<ShopSlot> slots = new();

    // 아이템 ID → 구매하려는 수량
    // 0개인 상품은 딕셔너리에 넣지 않음
    private readonly Dictionary<string, int> selectedAmounts = new();

    private PlayerShop shop;
    private PlayerMove move;

    private ShopData currentShop;

    private bool dirty;

    private int displayedMoney;
    private int displayedDay;

    public bool BOpen { get; private set; }
    public bool CanOpen => isActiveAndEnabled
        && !BOpen
        && BindPlayer()
        && shop.IsReady
        && !shop.IsUsingTool;

    private void Awake()
    {
        shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
        buyButton.onClick.AddListener(BuySelected);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
        buyButton.onClick.RemoveListener(BuySelected);

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

        if (displayedDay != shop.Day)
        {
            selectedAmounts.Clear();

            ShowMessage(
                "날짜가 변경되어 구매 수량을 초기화했습니다.");

            dirty = true;
        }

        if (displayedMoney != shop.Money)
            dirty = true;

        if (dirty)
            RefreshSlots();
    }

    private bool BindPlayer()
    {
        if (shop != null && move != null)
            return true;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError(
                "[ShopUI] Player를 찾을 수 없습니다.",
                this);

            return false;
        }

        shop = player.GetComponent<PlayerShop>();
        move = player.GetComponent<PlayerMove>();

        if (shop == null || move == null)
        {
            Debug.LogError(
                "[ShopUI] PlayerShop 또는 PlayerMove가 없습니다.",
                this);

            return false;
        }

        return true;
    }

    public void Open(ShopData data)
    {
        if (data == null || !CanOpen)
            return;

        if (!data.Validate(out string error))
        {
            Debug.LogError($"[ShopUI] {error}", this);
            return;
        }

        currentShop = data;

        selectedAmounts.Clear();

        if (inventoryUI != null)
            inventoryUI.SetOpen(false);

        BOpen = true;

        move.SetMoveEnabled(false);

        shop.Inventory.OnChanged += RequestRefresh;

        shopPanel.SetActive(true);

        ShowMessage("");
        RefreshSlots();
    }

    public void Close()
    {
        if (!BOpen)
            return;

        BOpen = false;

        if (shop != null && shop.Inventory != null)
            shop.Inventory.OnChanged -= RequestRefresh;

        if (move != null)
            move.SetMoveEnabled(true);

        selectedAmounts.Clear();

        shopPanel.SetActive(false);

        currentShop = null;
        dirty = false;
    }

    private void RequestRefresh()
    {
        dirty = true;
    }

    private ShopSlot GetSlot(int index)
    {
        while (slots.Count <= index)
        {
            ShopSlot slot =
                Instantiate(slotPrefab, slotRoot, false);

            slots.Add(slot);
        }

        ShopSlot result = slots[index];

        result.gameObject.SetActive(true);

        return result;
    }

    private Sprite GetIcon(ItemData_SO data)
    {
        if (data.Icon != null)
            return data.Icon;

        if (data.ItemType == ItemTypeSO.Tool)
            return shop.Inventory.GetToolData(data.Id)?.Icon;

        return null;
    }

    private int GetSelectedAmount(string itemId)
    {
        return selectedAmounts.TryGetValue(itemId, out int amount)
            ? amount
            : 0;
    }

    private void ChangeAmount(string itemId, int delta)
    {
        if (!BOpen || currentShop == null)
            return;

        int current = GetSelectedAmount(itemId);

        int remaining =
            shop.GetBuyRemaining(currentShop, itemId);

        // int 최대값에서 +1 하는 경우도 안전하게 처리
        long requested = (long)current + delta;

        int next = (int)System.Math.Max(
            0L,
            System.Math.Min(requested, (long)remaining));

        if (next == 0)
            selectedAmounts.Remove(itemId);
        else
            selectedAmounts[itemId] = next;

        ShowMessage("");
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        dirty = false;

        if (!BOpen || currentShop == null)
            return;

        displayedMoney = shop.Money;
        displayedDay = shop.Day;

        int visibleCount = 0;

        if (currentShop.buyItems != null)
        {
            foreach (ShopItemEntry entry in currentShop.buyItems)
            {
                if (entry == null)
                    continue;

                ItemData_SO data =
                    shop.Inventory.GetItemData(entry.itemId);

                if (data == null)
                {
                    selectedAmounts.Remove(entry.itemId);

                    Debug.LogWarning(
                        $"[ShopUI] ItemDatabase에 없는 ID: {entry.itemId}",
                        this);

                    continue;
                }

                int remaining =
                    shop.GetBuyRemaining(currentShop, entry.itemId);

                int amount = Mathf.Clamp(
                    GetSelectedAmount(entry.itemId),
                    0,
                    remaining);

                if (amount == 0)
                    selectedAmounts.Remove(entry.itemId);
                else
                    selectedAmounts[entry.itemId] = amount;

                string itemId = entry.itemId;

                ShopSlot slot = GetSlot(visibleCount);

                slot.Bind(
                    data,
                    GetIcon(data),
                    shop.GetOwnedCount(itemId),
                    amount,
                    remaining,
                    entry.dailyLimit < 0,
                    delta => ChangeAmount(itemId, delta));

                visibleCount++;
            }
        }

        for (int i = visibleCount; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        // 표시용 합계.
        // 여러 상품 합계를 넉넉하게 표현하기 위해 decimal 사용.
        decimal total = 0;
        long totalCount = 0;

        bool valid = selectedAmounts.Count > 0;

        foreach (KeyValuePair<string, int> selection in selectedAmounts)
        {
            ItemData_SO data =
                shop.Inventory.GetItemData(selection.Key);

            if (data == null || data.Buy < 0)
            {
                valid = false;
                continue;
            }

            if (!currentShop.CanBuy(selection.Key) ||
                selection.Value <= 0 ||
                selection.Value >
                    shop.GetBuyRemaining(currentShop, selection.Key))
            {
                valid = false;
            }

            total += (decimal)data.Buy * selection.Value;
            totalCount += selection.Value;
        }

        moneyText.text = $"소지금 {shop.Money:N0} G";

        totalPriceText.text = $"{total:N0} G";

        buyButton.interactable =
            valid &&
            shop.IsReady &&
            !shop.IsUsingTool &&
            total <= shop.Money;
    }

    private void BuySelected()
    {
        if (!BOpen || selectedAmounts.Count == 0)
            return;

        bool success = shop.TryBuySelected(
            currentShop,
            selectedAmounts,
            out string message);

        // 성공한 상품 수량이 다시 구매되지 않도록 초기화
        if (success)
            selectedAmounts.Clear();

        RefreshSlots();
        ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;
    }
}