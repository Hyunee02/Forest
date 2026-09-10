using System.Collections.Generic;
using UnityEngine;

public class ItemLoadManager : MonoBehaviour
{
    public static ItemLoadManager Instance { get; private set; }

    private Dictionary<string, ItemData> itemDict;
    private Dictionary<string, ToolData> toolDict;

    //private void Awake()
    //{
    //    Instance = this;
    //    LoadItemData();
    //}

    //private void LoadItemData()
    //{
    //    // json 파일 TextAsset으로 받아오기
    //    TextAsset jsonFile = Resources.Load<TextAsset>("Data/ItemData");

    //    //  json 파일 null 방지
    //    if (jsonFile == null)
    //    {
    //        Debug.LogError("ItemData.json is null");
    //        return;
    //    }

    //    // json items 배열을 ItemDataTable의 items 배열로 변환
    //    ItemDataTable table = JsonUtility.FromJson<ItemDataTable>(jsonFile.text);

    //    itemDict = new Dictionary<string, ItemData>();
    //    toolDict = new Dictionary<string, ToolData>();

    //    foreach (ItemData item in table.items)
    //    {
    //        if (itemDict.ContainsKey(item.id))
    //            continue;

    //        itemDict.Add(item.id, item);
    //    }

    //    foreach (ToolData tool in table.tools)
    //    {
    //        if (toolDict.ContainsKey(tool.itemId))
    //            continue;

    //        if (!itemDict.TryGetValue(tool.itemId, out ItemData item))
    //        {
    //            Debug.LogError($"Impossible to match ToolData : {tool.itemId}");
    //            continue;
    //        }

    //        if (item.itemType != ItemType.Tool)
    //        {
    //            Debug.LogError($"ToolData is connected to non-tool item : {tool.itemId}");
    //            continue;
    //        }

    //        toolDict.Add(tool.itemId, tool);
    //    }
    //}

    /// <summary>
    /// 아이템 데이터 사용
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemData GetItemData(string id)
    {
        if (itemDict.TryGetValue(id, out ItemData data))
            return data;

        return null;
    }

    /// <summary>
    /// 도구 데이터 로드
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public ToolData GetToolData(string itemId)
    {
        if (toolDict.TryGetValue(itemId, out ToolData data))
            return data;

        return null;
    }

    /// <summary>
    /// 아이템 이미지 로드
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Sprite GetItemSprite(string id)
    {
        ItemData data = GetItemData(id);

        if (data == null)
            return null;

        return ResourceLoader.Load<Sprite>(data.imagePath);
    }

    /// <summary>
    /// 아이템 프리팹 로드
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameObject GetItemPrefab(string id)
    {
        ItemData data = GetItemData(id);

        if (data == null)
            return null;

        return ResourceLoader.Load<GameObject>(data.prefabPath);
    }
}
