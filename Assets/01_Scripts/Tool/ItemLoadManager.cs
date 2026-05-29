using System.Collections.Generic;
using UnityEngine;

public class ItemLoadManager : MonoBehaviour
{
    public static ItemLoadManager Instance { get; private set; }

    private Dictionary<string, ItemData> itemDict;

    Dictionary<string, ItemData> ItemDict => itemDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadItemData();
    }

    private void LoadItemData()
    {
        // json 파일 TextAsset으로 받아오기
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/ItemData");

        //  json 파일 null 방지
        if (jsonFile == null)
        {
            Debug.LogError("ItemData.json is null");
            return;
        }

        // json items 배열을 ItemDataTable의 items 배열로 변환
        ItemDataTable table = JsonUtility.FromJson<ItemDataTable>(jsonFile.text);
        itemDict = new Dictionary<string, ItemData>();

        foreach (ItemData item in table.items)
        {
            if (itemDict.ContainsKey(item.id))
                continue;

            itemDict.Add(item.id, item);
        }
    }

    /// <summary>
    /// Path에 있는 파일 로드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// 아이템 데이터 사용
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemData GetItemData(string id)
    {
        if (itemDict.TryGetValue(id, out ItemData data))
        {
            return data;
        }

        return null;
    }
}
