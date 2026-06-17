//using UnityEngine;

//[RequireComponent(typeof(PlayerInventory))]
//public class PlayerEquip : MonoBehaviour
//{
//    [SerializeField] private Transform toolPos;
//    [SerializeField] private float useCollisionTime = 0.15f;

//    private PlayerInventory inventory;
//    private PlayerBindInput input;
//    private ToolBase curTool;

//    public string CurToolId => curTool.Id;

//    private void Awake()
//    {
//        inventory = GetComponent<PlayerInventory>();
//        input = GetComponent<PlayerBindInput>();

//        if (toolPos == null)
//            toolPos = transform.FindChildByName("ToolPos");
//    }

//    private void OnEnable()
//    {
//        input.OnAttackInput += UseCurrentTool;
//    }

//    private void OnDisable()
//    {
//        input.OnAttackInput -= UseCurrentTool;
//    }

//    /// <summary>
//    /// 도구 장착
//    /// </summary>
//    /// <param name="toolId"></param>
//    public void EquipTool(string itemId)
//    {
//        ItemData itemData = ItemLoadManager.Instance.GetItemData(itemId);
//        ToolData toolData = ItemLoadManager.Instance.GetToolData(itemId);
//        GameObject prefab = ItemLoadManager.Instance.GetItemPrefab(itemId);

//        if (itemData == null)
//        {
//            Debug.LogError($"ItemData is null\nID : {itemId}");
//            return;
//        }
        
//        if (toolData == null)
//        {
//            Debug.LogError($"ToolData is null\nID : {itemId}");
//            return;
//        }

//        // 중복 장착 방지
//        if (curTool != null)
//            UnEquipTool();



//        // 프리팹 찾기
//        //if (!toolPrefabDict.TryGetValue(curToolData.id, out ToolBase curToolPrefab))
//        //{
//        //    Debug.LogError($"ToolPrefab is null\nID : {curToolData.id}");
//        //    return;
//        //}

//        // 도구 생성
//        //GameObject curToolObj = Instantiate(curToolPrefab.gameObject, toolPos, false);
//        //curTool = curToolObj.GetComponent<ToolBase>();
//        //curToolObj.name = curToolData.name;

//        // 도구 초기화
//        curTool.transform.localPosition = Vector3.zero;
//        curTool.transform.localScale = Vector3.one;

//        // 데이터 적용
//        curTool.Init(curToolData);
//    }

//    /// <summary>
//    /// 도구 장착 해지
//    /// </summary>
//    public void UnEquipTool()
//    {
//        // 장착 도구 없으면 리턴
//        if (curTool == null)
//            return;

//        Destroy(curTool.gameObject);

//        curTool = null;
//        curToolData = null;
//    }
//}
