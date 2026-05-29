using System.Collections.Generic;
using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
    [SerializeField] private Transform toolPos;

    private ToolBase curTool;
    private ToolData curToolData;

    private void Awake()
    {
        toolPos = transform.FindChildByName("ToolPos");
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    /// <param name="toolId"></param>
    public void EquipTool(int toolId)
    {
        // id 받아와서 데이터 저장
        curToolData = ItemLoadManager.Instance.GetToolData(toolId);

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
        //if (!toolPrefabDict.TryGetValue(curToolData.id, out ToolBase curToolPrefab))
        //{
        //    Debug.LogError($"ToolPrefab is null\nID : {curToolData.id}");
        //    return;
        //}

        // 도구 생성
        //GameObject curToolObj = Instantiate(curToolPrefab.gameObject, toolPos, false);
        //curTool = curToolObj.GetComponent<ToolBase>();
        //curToolObj.name = curToolData.name;

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
