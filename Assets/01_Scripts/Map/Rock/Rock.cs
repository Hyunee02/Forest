using UnityEngine;

public class Rock : MapObject, IHitTarget
{
    [Header("Rock")]
    [SerializeField] private int maxHp;

    private int curHp;

    private void Start()
    {
        curHp = maxHp;
    }

    public bool CanHit(ToolType toolType)
    {
        return toolType == ToolType.Pickaxe;
    }

    public void Hit(int damage)
    {
        curHp -= damage;
        curHp = Mathf.Max(0, curHp);

        if (curHp <= 0)
            BreakRock();
    }

    private void BreakRock()
    {
        Destroy(gameObject);
    }
}
