using UnityEngine;

public enum ToolType
{
    None,
    Axe,
    Pickaxe,
    Hoe,
    WateringCan,
    FishingRod,
    Knife,
}

[CreateAssetMenu(fileName = "ToolData", menuName = "Data/ToolData")]
public class ToolData : ScriptableObject
{
    [SerializeField] private string toolName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject prefab;

    [SerializeField] private int buy;
    [SerializeField] private int sell;
    [SerializeField] private int stack = 1;

    [SerializeField] private int power;
    [SerializeField] private int durability;
    [SerializeField] private float cooldown;

    public string ToolName => toolName;
    public string Description => description;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
    
    public int Buy => buy;
    public int Sell => sell;

    public int Power => power;
    public int Durability => durability;
    public float Cooldown => cooldown;
}
