using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SellSlot : MonoBehaviour
{
    [Header("물품 표시")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;

    [Header("판매 수량")]
    [SerializeField] private TMP_Text saleCountText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;

    [Header("오른쪽 판매 목록에서 사용")]
    [SerializeField] private TMP_Text priceText;

    [Header("선택 표시")]
    [SerializeField] private GameObject selectedMark;

    private Button button;

    private Action onClick;
    private Action<int> onChangeAmount;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);

        if (minusButton != null)
            minusButton.onClick.AddListener(Decrease);

        if (plusButton != null)
            plusButton.onClick.AddListener(Increase);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);

        if (minusButton != null)
            minusButton.onClick.RemoveListener(Decrease);

        if (plusButton != null)
            plusButton.onClick.RemoveListener(Increase);

        onClick = null;
        onChangeAmount = null;
    }

    public void Bind(
        ItemData_SO data,
        Sprite sprite,
        int ownedCount,
        int selectedAmount,
        bool showPrice,
        Action clickAction,
        Action<int> changeAmountAction)
    {
        onClick = clickAction;
        onChangeAmount = changeAmountAction;

        button.interactable = clickAction != null;

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        if (nameText != null)
            nameText.text = data.ItemName;

        if (countText != null)
        {
            // 왼쪽: 보유 수량 / 오른쪽: 판매 선택 수량
            int displayCount = showPrice
                ? selectedAmount
                : ownedCount;

            countText.text = $"× {displayCount:N0}";
        }

        if (saleCountText != null)
        {
            saleCountText.text =
                $"{selectedAmount:N0}/{ownedCount:N0}";
        }

        if (priceText != null)
        {
            priceText.gameObject.SetActive(showPrice);

            long total = (long)data.SellPrice * selectedAmount;
            priceText.text = $"{total:N0} G";
        }

        if (selectedMark != null)
        {
            // 왼쪽 슬롯에서만 선택 표시
            selectedMark.SetActive(
                !showPrice && selectedAmount > 0);
        }

        bool canChange =
            data.SellPrice >= 0 &&
            onChangeAmount != null;

        if (minusButton != null)
        {
            minusButton.interactable =
                canChange && selectedAmount > 0;
        }

        if (plusButton != null)
        {
            plusButton.interactable =
                canChange && selectedAmount < ownedCount;
        }
    }

    private void HandleClick()
    {
        onClick?.Invoke();
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