using System.Collections.Generic;
using UnityEngine;

public class ToolLoadManager : MonoBehaviour
{
    public static ToolLoadManager Instance { get; private set; }

    private Dictionary<int, ToolData> toolDict;

    Dictionary<int, ToolData> ToolDict => toolDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadToolData();
    }

    private void LoadToolData()
    {
        // json 파일 TextAsset으로 받아오기
        TextAsset jsonFile = Resources.Load<TextAsset>("ToolData");

        //  json 파일 null 방지
        if (jsonFile == null)
        {
            Debug.LogError("ToolData.json is null");
            return;
        }

        // json tools 배열을 ToolDataTable의 tools 배열로 변환
        ToolDataTable table = JsonUtility.FromJson<ToolDataTable>(jsonFile.text);
        toolDict = new Dictionary<int, ToolData>();

        // 딕셔너리에 넣어주기
        foreach (ToolData tool in table.tools)
        {
            if (toolDict.ContainsKey(tool.id))
                continue;

            toolDict.Add(tool.id, tool);
        }
    }

    /// <summary>
    /// 도구 데이터 사용
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ToolData GetToolData(int id)
    {
        if (toolDict.TryGetValue(id, out ToolData data))
        {
            return data;
        }

        return null;
    }
}
