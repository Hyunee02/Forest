using UnityEngine;

public class Tool_WateringCan : ToolBase
{
    [Header("Water")]
    [SerializeField, Min(1)] private int maxWater;
    [SerializeField, Min(0)] private int curWater;

    [SerializeField] private LayerMask soilLayer;

    [SerializeField] private GameObject waterEffect;

    public override ToolType ToolType => ToolType.WateringCan;

    public int MaxWater => maxWater;
    public int CurWater => curWater;

    public override void Init(PlayerEquip equip, Transform root, ToolData data)
    {
        base.Init(equip, root, data);

        maxWater = CurDurability;
        curWater = Mathf.Clamp(curWater, 0, maxWater);
    }
}
