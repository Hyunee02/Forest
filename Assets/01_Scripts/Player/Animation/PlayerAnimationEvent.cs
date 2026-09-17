using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    private PlayerMove move;
    private PlayerEquip equip;

    private void Awake()
    {
        move = GetComponentInParent<PlayerMove>();
        equip = GetComponentInParent<PlayerEquip>();
    }

    #region ----- 도구 충돌 애니메이션

    /// <summary>
    /// 애니메이션 도구 사용 시작
    /// </summary>
    public void BeginUseTool()
    {
        move.SetMoveEnabled(false);
    }

    /// <summary>
    /// 애니메이션 타격 시작
    /// </summary>
    public void BeginToolCollsion()
    {
        if (equip == null || equip.CurTool == null)
            return;

        equip.CurTool.BeginCollision();
    }

    /// <summary>
    /// 애니메이션 실제 타격 끝
    /// </summary>
    public void EndToolCollision()
    {
        if (equip == null || equip.CurTool == null)
            return;

        equip.CurTool.EndCollision();
    }

    /// <summary>
    /// 애니메이션 도구 사용 끝
    /// </summary>
    public void EndUseTool()
    {
        if (equip == null)
            return;

        equip.EndUseTool();

        move.SetMoveEnabled(true);
    }
    #endregion
}
