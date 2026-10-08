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
    private string id;
    private ItemTypeSO itemType;

    private string itemName;
    private int buy;
    private int sell;

    [TextArea]
    private string description;

    [Header("<< Inventory >>")]
    private Sprite icon;
    private int maxStack = 30;

    [Header("<< World >>")]
    private GameObject prefab;

    public string Id => id;
    public ItemTypeSO ItemType => itemType;

    public string ItemName => itemName;
    public int Buy => buy;
    public int Sell => sell;

    public string Description => description;

    public Sprite Icon => icon;
    public int MaxStack => maxStack;

    public GameObject Prefab => prefab;
}