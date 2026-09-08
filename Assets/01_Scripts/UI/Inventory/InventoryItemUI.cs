using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;

    public void SetItem(ItemData_SO itemData, int count)
    {
        if (itemData == null)
            return;

        icon.sprite = itemData.icon;

        if (count > 1)
            countText.text = count.ToString();
        else
            countText.text = "";
    }
}