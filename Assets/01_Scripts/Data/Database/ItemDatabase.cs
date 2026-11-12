
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("<< Item >>")]
    [SerializeField] private ItemData_SO[] items;

    [Header("<< Tool >>")]
    [SerializeField] private ToolData[] tools;

    public ItemData_SO GetItemData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        foreach (ItemData_SO item in items)
        {
            if (item == null)
                continue;

            if (item.Id == id)
                return item;
        }

        return null;
    }

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

#if UNITY_EDITOR
    [ContextMenu("아이템 데이터 자동 등록")]
    private void RefreshItems()
    {
        // 일반 아이템 자동 등록
        string[] itemGuids = AssetDatabase.FindAssets("t:ItemData_SO");

        System.Collections.Generic.List<ItemData_SO> itemList = new();

        foreach (string guid in itemGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData_SO item = AssetDatabase.LoadAssetAtPath<ItemData_SO>(path);

            if (item != null)
                itemList.Add(item);
        }

        items = itemList.ToArray();


        // 도구 데이터 자동 등록
        string[] toolGuids = AssetDatabase.FindAssets("t:ToolData");

        System.Collections.Generic.List<ToolData> toolList = new();

        foreach (string guid in toolGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ToolData tool = AssetDatabase.LoadAssetAtPath<ToolData>(path);

            if (tool != null)
                toolList.Add(tool);
        }

        tools = toolList.ToArray();


        // 저장
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"아이템 {items.Length}개 / 도구 {tools.Length}개 등록 완료!",
            this
        );
    }
#endif

}
