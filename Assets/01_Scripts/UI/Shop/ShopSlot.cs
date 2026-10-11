using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text ownedCountText;
    [SerializeField] private TMP_Text purchaseText;
    [SerializeField] private TMP_Text priceText;

    private Button button;
    private Action onBuy;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleClick);
    }

    /// <summary>
    /// 데이터 상점 슬롯 표시
    /// </summary>
    /// <param name="data">아이템 데이터</param>
    /// <param name="sprite">아이템 아이콘</param>
    /// <param name="ownedCount">플레이어 보유 수량</param>
    /// <param name="buyAction">구매 버튼 클릭 시 실행할 함수</param>
    public void Bind(ItemData_SO data, Sprite sprite, int ownedCount, Action buyAction)
    {
        onBuy = buyAction;

        nameText.text = data.ItemName;

        icon.sprite = sprite;
        icon.enabled = sprite != null;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        ownedCountText.text = ownedCount.ToString();
        purchaseText.text = "1개 구매";
        priceText.text = data.Buy.ToString("N0");

        button.interactable = data.Buy >= 0 && onBuy != null;
    }

    private void HandleClick()
    {
        onBuy?.Invoke();
    }
}
