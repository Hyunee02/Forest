using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
    [SerializeField] private ITool[] tools;

    [SerializeField] private Transform toolPos;

    private Animator animator;

    private bool bEquip;
    private bool bUse;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        
    }

    private void Update_Equip()
    {

    }

    public void GetTool(int toolId)
    {
        ToolData curData = ToolLoadManager.Instance.GetToolData(toolId);


    }


}
