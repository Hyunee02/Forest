using UnityEngine;

public enum TreeType
{
    Normal,
    Fruit
}

public enum TreeState
{
    Seed,
    Sapling,
    Grown
}

public enum Season
{
    SpringSummer,
    Fall,
    Winter
}

/// <summary>
/// 
/// 추가해야할 것
/// 
/// 1. 계절 별 나무 visual
/// 2. Planted Day와 isWatered에 따른 Tree State
/// 
/// </summary>
public class Tree : MapObject
{
    [Header("<< 나무 정보 >>")]
    [SerializeField] private TreeType treeType = TreeType.Normal;
    [SerializeField] private TreeState treeState = TreeState.Grown;
    [SerializeField] private Season season = Season.SpringSummer;

    [Header("<< Normal 나무 >>")]
    [SerializeField] private GameObject springSummerNormal;
    [SerializeField] private GameObject fallNormal;
    [SerializeField] private GameObject winterNormal;

    [Header("<< Fruit 나무 >>")]
    [SerializeField] private GameObject springSummerFruit;
    [SerializeField] private GameObject fallFruit;
    [SerializeField] private GameObject winterFruit;

    [Header("<< Normal 나무 성장 크기 >>")]
    [SerializeField] private Vector3 normalSeedScale = new Vector3(0.15f, 0.15f, 0.15f);
    [SerializeField] private Vector3 normalSaplingScale = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 normalGrownScale = new Vector3(0.8f, 0.8f, 0.8f);

    [Header("<< Fruit 나무 성장 크기 >>")]
    [SerializeField] private Vector3 fruitSeedScale = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private Vector3 fruitSaplingScale = new Vector3(0.3f, 0.3f, 0.3f);
    [SerializeField] private Vector3 fruitGrownScale = new Vector3(0.6f, 0.6f, 0.6f);

    [Header("<< 나무 성장 조건 >>")]
    [SerializeField] private int plantedDay;
    [SerializeField] private bool isWateredToday;

    private Vector2Int tilePosition;

    public TreeType TreeType => treeType;
    public TreeState TreeState => treeState;
    public Vector2Int TilePosition => tilePosition;

    public bool IsGrown => treeState == TreeState.Grown;

    public void Initialize(TreeType type, TreeState state, Vector2Int tilePos, int currentDay)
    {
        treeType = type;
        treeState = state;
        tilePosition = tilePos;
        plantedDay = currentDay;
        isWateredToday = false;

        ApplyScaleByState();
    }

    public void Water()
    {
        if (treeState == TreeState.Grown)
            return;

        isWateredToday = true;
    }

    public void GrowNextDay()
    {
        if (!isWateredToday)
            return;

        if (treeState == TreeState.Seed)        
            SetState(TreeState.Sapling);
        
        else if (treeState == TreeState.Sapling)        
            SetState(TreeState.Grown);        

        isWateredToday = false;
    }

    public void SetTilePosition(Vector2Int tilePos)
    {
        tilePosition = tilePos;
    }

    private void SetState(TreeState newState)
    {
        treeState = newState;

        ApplyScaleByState();
    }

    private void ApplyScaleByState()
    {
        if (treeType == TreeType.Normal)
        {
            switch (treeState)
            {
                case TreeState.Seed:
                    transform.localScale = normalSeedScale;
                    break;

                case TreeState.Sapling:
                    transform.localScale = normalSaplingScale;
                    break;

                case TreeState.Grown:
                    transform.localScale = normalGrownScale;
                    break;
            }
        }

        else if (treeType == TreeType.Fruit)
        {
            switch (treeState)
            {
                case TreeState.Seed:
                    transform.localScale = fruitSeedScale;
                    break;

                case TreeState.Sapling:
                    transform.localScale = fruitSaplingScale ;
                    break;

                case TreeState.Grown:
                    transform.localScale = fruitGrownScale;
                    break;
            }
        }
    }
}
