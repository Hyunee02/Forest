using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    private PlayerEquip equip;

    private void Awake()
    {
        equip = GetComponentInParent<PlayerEquip>();
    }

    #region ----- 도구 충돌 애니메이션

    /// <summary>
    /// 애니메이션 타격 시작
    /// </summary>
    public void BeginToolCollsion()
    {
        ToolBase tool = equip.CurTool;

        if (tool == null)
            return;

        tool.BeginCollision();
    }

    /// <summary>
    /// 애니메이션 실제 타격 끝
    /// </summary>
    public void EndToolCollision()
    {
        ToolBase tool = equip.CurTool;

        if (tool == null)
            return;

        tool.EndCollision();
    }

    /// <summary>
    /// 애니메이션 도구 사용 끝
    /// </summary>
    public void EndUseTool()
    {
        ToolBase tool = equip.CurTool;

        if (tool == null)
            return;

        tool.EndUse();
    }
    #endregion
}
