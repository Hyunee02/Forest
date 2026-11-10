using UnityEngine;

public enum ItemTypeSO
{
    None,
    Tool,
    Material,
    Food,
    Furniture,
    Etc,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemData_SO : ScriptableObject
{
    [Header("<< Basic >>")]
    [SerializeField] private string id;
    [SerializeField] private ItemTypeSO itemType;
    [SerializeField] private string itemName;

    [Header("<< Price >>")]
    [SerializeField] public bool canBuy = true;
    [SerializeField, Min(0)] private int buyPrice;

    [SerializeField] public bool canSell = true;
    [SerializeField, Min(0)] private int sellPrice;

    [TextArea]
    [SerializeField] private string description;

    [Header("<< Inventory >>")]
    [SerializeField] private Sprite icon;
    [SerializeField, Min(1)] private int maxStack = 30;

    [Header("<< World >>")]
    [SerializeField] private GameObject prefab;

    public string Id => id;
    public ItemTypeSO ItemType => itemType;

    public string ItemName => itemName;
    public int BuyPrice => buyPrice;
    public int SellPrice => Mathf.FloorToInt(buyPrice * 0.3f);

    public string Description => description;

    public Sprite Icon => icon;
    public int MaxStack => maxStack;

    public GameObject Prefab => prefab;

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