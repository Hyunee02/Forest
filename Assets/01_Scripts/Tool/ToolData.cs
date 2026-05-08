using System;

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
public class ToolData
{
    public int id;
    public string toolType;
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