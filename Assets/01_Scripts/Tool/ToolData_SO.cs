using UnityEngine;

public enum ToolTypeSO
{
    None,
    Axe,
    Hoe,
    Pickaxe,
    Shovel,
    WateringCan,
    FishingRod,
}

[CreateAssetMenu(fileName = "ToolData", menuName = "Game/Tool Data")]
public class ToolData_SO : ScriptableObject
{
    [Header("<< Item >>")]
    public ItemData_SO itemData;

    [Header("<< Tool >>")]
    public ToolTypeSO toolType;

    [Header("<< Tool Stat >>")]
    public int rate;
    public int durability;
    public int reduce;
}