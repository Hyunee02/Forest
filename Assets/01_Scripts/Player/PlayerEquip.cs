using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ToolMapping
{
    public int id;
    public ToolBase prefab;
}

public class PlayerEquip : MonoBehaviour
{
    [SerializeField] private ToolMapping[] toolPrefabs;

    [SerializeField] private Transform toolPos;


    private Dictionary<int, ToolBase> toolPrefabDict;

    private ToolBase curTool;
    private ToolData curToolData;

    private void Awake()
    {
        toolPos = transform.FindChildByName("ToolPos");

        InitPrefabDict();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipTool(3);
    }

    /// <summary>
    /// 프리팹 딕셔너리 초기화
    /// </summary>
    private void InitPrefabDict()
    {
        toolPrefabDict = new Dictionary<int, ToolBase>();

        // Prefab Dict 초기화
        foreach (ToolMapping toolMap in toolPrefabs)
        {
            // 프리팹 널 방지
            if (toolMap.prefab == null)
                continue;

            // 중복 방지
            if (toolPrefabDict.ContainsKey(toolMap.id))
                continue;

            toolPrefabDict.Add(toolMap.id, toolMap.prefab);
        }
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    /// <param name="toolId"></param>
    public void EquipTool(int toolId)
    {
        // id 받아와서 데이터 저장
        curToolData = ToolLoadManager.Instance.GetToolData(toolId);

        // data null 방지
        if (curToolData == null)
        {
            Debug.LogError($"ToolData is null\nID : {toolId}");
            return;
        }

        // 중복 장착 방지
        if (curTool != null)
            UnEquipTool();

        // 프리팹 찾기
        if (!toolPrefabDict.TryGetValue(curToolData.id, out ToolBase curToolPrefab))
        {
            Debug.LogError($"ToolPrefab is null\nID : {curToolData.id}");
            return;
        }

        // 도구 생성
        GameObject curToolObj = Instantiate(curToolPrefab.gameObject, toolPos, false);
        curTool = curToolObj.GetComponent<ToolBase>();
        curToolObj.name = curToolData.name;

        // 도구 초기화
        curTool.transform.localPosition = Vector3.zero;
        curTool.transform.localScale = Vector3.one;

        // 데이터 적용
        curTool.Init(curToolData);
    }

    /// <summary>
    /// 도구 장착 해지
    /// </summary>
    public void UnEquipTool()
    {
        // 장착 도구 없으면 리턴
        if (curTool == null)
            return;

        Destroy(curTool.gameObject);

        curTool = null;
        curToolData = null;
    }
}
