using UnityEngine;

public enum ItemTypeSO
{
    None,
    Tool,
    Material,
    Fish,
    Food,
    Furniture,
    Etc,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemData_SO : ScriptableObject
{
    [Header("<< Basic >>")]
    public string id;
    public ItemTypeSO itemType;
    public string itemName;

    [Header("<< Price >>")]
    public bool canBuy = true;
    public int buyPrice;

    public bool canSell = true;
    public int sellPrice;

    [TextArea]
    public string description;

    [Header("<< Inventory >>")]
    public Sprite icon;
    public int maxStack = 30;

    [Header("<< World >>")]
    public GameObject prefab;

    public int SellPrice => Mathf.FloorToInt(buyPrice * 0.3f);

    private void OnValidate()
    {
        if (!canBuy)
        {
            buyPrice = 0;
        }

        if (canBuy)
        {
            sellPrice = Mathf.FloorToInt(buyPrice * 0.3f);
        }
    }
}