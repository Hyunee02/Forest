using System;

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
    public int id;
    public ItemType itemType;
    public string name;
}

[Serializable]
public class ToolData
{
    public int id;
    public ToolType toolType;
    public string name;
    public int rate;
    public int durability;
    public int reduce;
}

[Serializable]
public class ToolDataTable
{
    public ToolData[] tools;
}