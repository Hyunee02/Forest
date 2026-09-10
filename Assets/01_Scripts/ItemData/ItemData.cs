
using System;

public enum ItemType
{
    None = 0,
    Tool,
    Material,
    Food,
    Furniture,
    Etc,
}

//public enum ToolType
//{
//    None = 0,
//    Axe,
//    Hoe,
//    Pickaxe,
//    Shovel,
//    WateringCan,
//    FishingRod,
//}

[Serializable]
public class ItemData
{
    public string id;
    public ItemType itemType;

    public string name;
    public int price;
    public string description;

    public string imagePath;
    public string prefabPath;

    public int maxStack = 1;
}

//[Serializable]
//public class ToolData
//{
//    public string itemId;
//    public ToolType toolType;

//    public int rate;
//    public int durability;
//    public int reduce;
//}

[Serializable]
public class ItemDataTable
{
    public ItemData[] items;
    public ToolData[] tools;
}
