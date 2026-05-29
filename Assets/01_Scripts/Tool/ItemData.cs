using System;
using UnityEngine;

public enum ItemType
{
    None,
    Tool,
    Material,
    Food,
    Furniture,
    Etc,
}

public enum ToolType
{
    None,
    Axe,
    Hoe,
    Pickaxe,
    Shovel,
    WateringCan,
    FishingRod,
}

[Serializable]
public class ItemData
{
    public string id;
    public ItemType itemType;
    public string name;
    public int price;
    public string description;
    public ToolInfo toolInfo;

    public string imagePath;
    public string prefabPath;
}

[Serializable]
public class ToolInfo
{
    public string id;
    public ToolType toolType;
    public int rate;
    public int durability;
    public int reduce;
}

[Serializable]
public class ItemDataTable
{
    public ItemData[] items;
}