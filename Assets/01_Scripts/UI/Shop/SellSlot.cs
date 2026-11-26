using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class SellSlot : MonoBehaviour
{
    [Header("물품 표시")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Color originalIconColor;

    [Header("판매 수량")]
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
        canvasGroup = GetComponent<CanvasGroup>();

        if (icon != null)
            originalIconColor = icon.color;
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

        bool isSelectedList = showPrice;
        bool canSell = data.canSell && data.SellPrice >= 0;

        canvasGroup.alpha = canSell ? 1f : 0.4f;
        canvasGroup.interactable = canSell;
        button.interactable = canSell && onClick != null;

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            icon.color = canSell ? originalIconColor : Color.gray;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        if (nameText != null)
            nameText.text = data.ItemName;

        if (countText != null)
        {
            countText.gameObject.SetActive(true);
            countText.text = $"{selectedAmount:N0}/{ownedCount:N0}";
        }

        if (priceText != null)
        {
            priceText.gameObject.SetActive(true);
            priceText.text = $"{data.SellPrice:N0} G";
        }

        if (selectedMark != null)
            selectedMark.SetActive(!isSelectedList && selectedAmount > 0);

        // 연결한 버튼만 사용하고, 오른쪽에서는 항상 숨긴다.
        bool showButtons = !isSelectedList && onChangeAmount != null;

        if (minusButton != null)
        {
            minusButton.gameObject.SetActive(showButtons);
            minusButton.interactable = canSell && selectedAmount > 0;
        }

        if (plusButton != null)
        {
            plusButton.gameObject.SetActive(showButtons);
            plusButton.interactable = canSell && selectedAmount < ownedCount;
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