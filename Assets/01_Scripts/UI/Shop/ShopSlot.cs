using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("상품 정보")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text ownedCountText;
    [SerializeField] private TMP_Text priceText;

    [Header("구매 수량")]
    [SerializeField] private TMP_Text purchaseText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;

    // 수량 변경 요청: 감소 -1, 증가 +1
    private Action<int> onChangeAmount;

    private void OnEnable()
    {
        minusButton.onClick.AddListener(Decrease);
        plusButton.onClick.AddListener(Increase);
    }

    private void OnDisable()
    {
        minusButton.onClick.RemoveListener(Decrease);
        plusButton.onClick.RemoveListener(Increase);
    }

    /// <summary>
    /// 상품 정보와 구매 선택 상태 표시
    /// </summary>
    public void Bind(
        ItemData_SO data,
        Sprite sprite,
        long ownedCount,
        int selectedAmount,
        int remaining,
        bool unlimited,
        Action<int> changeAmountAction)
    {
        onChangeAmount = changeAmountAction;

        // 상품 이름
        nameText.text = data.ItemName;

        // 가방 아이콘 옆: 보유 수량
        ownedCountText.text = ownedCount.ToString("N0");

        // 동전 아이콘 옆: 개당 가격
        priceText.text = data.BuyPrice.ToString("N0");

        // 상품 이미지
        icon.sprite = sprite;
        icon.enabled = sprite != null;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        // 구매할 수량 / 오늘 남은 구매 가능 수량
        string remainingText = unlimited
            ? "∞"
            : remaining.ToString("N0");

        purchaseText.text =
            $"{selectedAmount:N0}/{remainingText}";

        bool canChange =
            data.BuyPrice >= 0 &&
            onChangeAmount != null;

        minusButton.interactable =
            canChange && selectedAmount > 0;

        plusButton.interactable =
            canChange && selectedAmount < remaining;
    }

    private void Decrease()
    {
        onChangeAmount?.Invoke(-1);
    }

    private void Increase()
    {
        onChangeAmount?.Invoke(1);
    }
}