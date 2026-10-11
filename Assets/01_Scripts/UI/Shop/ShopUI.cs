using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeButton;

    [Header("Shop List")]
    [SerializeField] private Transform slotRoot;
    [SerializeField] private ShopSlot slotPrefab;

    [Header("Text")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text messageText;

    [Header("InventoryUI")]
    [SerializeField] private InventoryUI inventoryUI;

    private readonly List<ShopSlot> slots = new();

    private PlayerShop shop;
    private PlayerMove move;

    private ShopData currentShop;

    private bool dirty;

    public bool BOpen { get; private set; }

    public bool CanOpen => isActiveAndEnabled
        && !BOpen
        && BindPlayer()
        && !shop.IsUsingTool;

    private void Awake()
    {
        shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
        Close();
    }

    private void Update()
    {
        if (BOpen
            && Keyboard.current != null
            && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void LateUpdate()
    {
        if (!BOpen)
            return;

        if (dirty)
            RefreshSlots();

        // 다른 기능에서 소지금 바뀔 시 표시
        string money = shop.Money.ToString("N0");

        if (moneyText.text != money)
            moneyText.text = money;
    }

    /// <summary>
    /// 플레이어 연결
    /// </summary>
    /// <returns></returns>
    private bool BindPlayer()
    {
        if (shop != null && move != null)
            return true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[ShopUI] Player를 찾을 수 없습니다.", this);
            return false;
        }

        shop = player.GetComponent<PlayerShop>();
        move = player.GetComponent<PlayerMove>();

        if (shop == null || move == null)
        {
            Debug.LogError("[ShopUI] PlayerShop 또는 PlayerMove가 없습니다.", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 상점 열기
    /// </summary>
    /// <param name="data"></param>
    public void Open(ShopData data)
    {
        if (data == null || !CanOpen)
            return;

        currentShop = data;

        // 인벤토리 닫기
        if (inventoryUI != null)
            inventoryUI.SetOpen(false);

        BOpen = true;

        move.SetMoveEnabled(false);
        shop.Inventory.OnChanged += RequestRefresh;

        shopPanel.SetActive(true);

        ShowMessage("");
        RefreshSlots();
        moneyText.text = shop.Money.ToString("N0");
    }

    /// <summary>
    /// 상점 닫기
    /// </summary>
    public void Close()
    {
        if (!BOpen)
            return;

        BOpen = false;

        if (shop != null && shop.Inventory != null)
            shop.Inventory.OnChanged -= RequestRefresh;

        if (move != null)
            move.SetMoveEnabled(true);

        shopPanel.SetActive(false);

        currentShop = null;
        dirty = false;
    }

    private void RequestRefresh()
    {
        dirty = true;
    }

    /// <summary>
    /// 슬롯 생성 및 재사용
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private ShopSlot GetSlot(int index)
    {
        // 현재 슬롯 개수가 요청한 인덱스보다 작거나 같으면 슬롯 추가
        while (slots.Count <= index)
        {
            ShopSlot slot = Instantiate(slotPrefab, slotRoot, false);
            slots.Add(slot);
        }

        ShopSlot result = slots[index];
        result.gameObject.SetActive(true);

        return result;
    }

    /// <summary>
    /// 아이템 이미지 가져오기
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private Sprite GetIcon(ItemData_SO data)
    {
        if (data.Icon != null)
            return data.Icon;

        if (data.ItemType == ItemTypeSO.Tool)
            return shop.Inventory.GetToolData(data.Id)?.Icon;

        return null;
    }

    /// <summary>
    /// 상품 목록 갱신
    /// </summary>
    private void RefreshSlots()
    {
        dirty = false;

        if (!BOpen || currentShop == null)
            return;

        int visibleCount = 0;

        if (currentShop.buyItemIds != null)
        {
            // 현재 상점 아이템 목록 순회
            foreach (string itemId in currentShop.buyItemIds)
            {
                ItemData_SO data = shop.Inventory.GetItemData(itemId);

                if (data == null)
                {
                    Debug.LogWarning($"[ShopUI] ItemDatabase 없는 ID : {itemId}", this);
                    continue;
                }

                string id = itemId;
                ShopSlot slot = GetSlot(visibleCount);

                slot.Bind(data, GetIcon(data), shop.GetOwnedCount(id), () => Buy(id));

                visibleCount++;
            }
        }

        // 이전 상점에서 사용했던 남는 슬롯 숨기기
        for (int i = visibleCount; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);
    }

    /// <summary>
    /// 구매 처리
    /// </summary>
    /// <param name="itemId"></param>
    private void Buy(string itemId)
    {
        if (!BOpen)
            return;

        shop.TryBuy(currentShop, itemId, out string message);

        ShowMessage(message);
        RefreshSlots();

        moneyText.text = shop.Money.ToString("N0");
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;
    }
}
