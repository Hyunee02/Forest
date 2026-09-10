using UnityEngine;

[RequireComponent(typeof(PlayerBindInput))]
public class PlayerEquip : MonoBehaviour
{
    [Header("Equip")]
    [SerializeField] private Transform handSocket;
    [SerializeField] private Transform rootObject;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string equipTriggerName;

    [Header("Test")]
    [SerializeField] private ToolData axeData;

    private PlayerBindInput input;

    private GameObject curToolObject;
    private ToolBase curTool;
    private ToolData curToolData;

    public ToolBase CurTool => curTool;
    public ToolData CurToolData => curToolData;

    public bool bTool => curTool != null;

    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();

        if (rootObject == null)
            rootObject = transform.root;

        if (handSocket == null)
            handSocket = Helper.FindChildByName(this.transform, "HandSocket");

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
            EquipTool(axeData);
    }

    private void OnEnable()
    {
        input.OnUseInput += UseTool;
    }

    private void OnDisable()
    {
        input.OnUseInput -= UseTool;
    }

    /// <summary>
    /// 도구 장착
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool EquipTool(ToolData toolData)
    {
        bool bEquip = toolData == null
            || toolData.Prefab == null
            || handSocket == null;

        if (bEquip)
            return false;

        // 도구 해제
        UnEquipTool();

        // 도구 프리팹 생성 및 위치 조정
        curToolObject = Instantiate(toolData.Prefab, handSocket, false);
        //curTool.transform.localPosition = Vector3.zero;
        //curTool.transform.localRotation = Quaternion.identity;
        curTool = curToolObject.GetComponent<ToolBase>();

        // 현재 도구 null 방지
        if (curTool == null)
        {
            Destroy(curToolObject);
            ClearCurrentTool();
            return false;
        }

        curToolData = toolData;
        curTool.Init(this, rootObject, curToolData);

        if (animator != null)
            animator.SetInteger("ToolType", (int)curTool.ToolType);

        return true;
    }

    /// <summary>
    /// 도구 장착 해제
    /// </summary>
    public void UnEquipTool()
    {
        // 도구 파괴
        if (curToolObject != null)
            Destroy(curToolObject);

        // 현재 도구 정보 초기화
        ClearCurrentTool();
    }

    /// <summary>
    /// 도구 사용
    /// </summary>
    public void UseTool()
    {
        if (curTool == null || curToolData == null)
            return;

        // 사용 가능 여부 검사
        if (!curTool.TryUse())
            return;

        animator.SetInteger("ToolType", (int)curTool.ToolType);

        animator.SetTrigger(equipTriggerName);
    }

    /// <summary>
    /// 현재 아이템 초기화
    /// </summary>
    private void ClearCurrentTool()
    {
        curToolObject = null;
        curTool = null;
        curToolData = null;

        animator.SetInteger("ToolType", 0);
    }
}
