using UnityEngine;

public class Tool_FishingRod : ToolBase
{
    [Header("³¬½ËÁÙ ½ÃÀÛ À§Ä¡")]
    [SerializeField] private Transform lineStart;

    public override ToolType ToolType => ToolType.FishingRod;

    protected override void OnTriggerEnter(Collider other)
    {
    }
}