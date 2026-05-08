using UnityEngine;


public abstract class MapObject : MonoBehaviour
{
    [Header("<< Map Object >>")]
    [SerializeField] protected int width = 1;
    [SerializeField] protected int height = 1;
    [SerializeField] protected bool movable = true;

    public int Width => width;
    public int Height => height;
    public bool Movable => movable;
}
