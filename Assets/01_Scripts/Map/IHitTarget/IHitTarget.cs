public interface IHitTarget
{
    bool CanHit(ToolType toolType);

    void Hit(int damage);
}