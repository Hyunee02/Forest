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
    public string id;
    public ItemTypeSO itemType;

    public string itemName;
    public int price;

    [TextArea]
    public string description;

    [Header("<< Inventory >>")]
    public Sprite icon;
    public int maxStack = 30;

    [Header("<< World >>")]
    public GameObject prefab;
}