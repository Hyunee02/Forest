using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("<< Item >>")]
    [SerializeField] private ItemData_SO[] items;

    [Header("<< Tool >>")]
    [SerializeField] private ToolData[] tools;

    /// <summary>
    /// ID로 아이템 데이터 가져오기
    /// </summary>
    public ItemData_SO GetItemData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        foreach (ItemData_SO item in items)
        {
            if (item == null)
                continue;

            if (item.id == id)
                return item;
        }

        return null;
    }

    /// <summary>
    /// ID로 도구 데이터 가져오기
    /// </summary>
    public ToolData GetToolData(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        foreach (ToolData tool in tools)
        {
            if (tool == null)
                continue;

            if (tool.ID == itemId)
                return tool;
        }

        return null;
    }
}