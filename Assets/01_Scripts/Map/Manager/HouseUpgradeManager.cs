using UnityEngine;

public class HouseUpgradeManager : MonoBehaviour
{
    public static HouseUpgradeManager Instance { get; private set; }

    [Header("<< 현재 집 레벨 >>")]
    [SerializeField] private int houseLevel = 0;

    public int HouseLevel => houseLevel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadHouseLevel();
    }

    public void UpgradeHouse()
    {
        houseLevel++;

        if (houseLevel > 3)
            houseLevel = 3;

        SaveHouseLevel();
    }

    public Vector2Int GetInteriorSize()
    {
        switch (houseLevel)
        {
            case 1:
                return new Vector2Int(6, 6);

            case 2:
                return new Vector2Int(8, 8);

            case 3:
                return new Vector2Int(12, 12);

            default:
                return new Vector2Int(0, 0);
        }
    }

    private void SaveHouseLevel()
    {
        PlayerPrefs.SetInt("HouseLevel", houseLevel);
        PlayerPrefs.Save();
    }

    private void LoadHouseLevel()
    {
        houseLevel = PlayerPrefs.GetInt("HouseLevel", 0);
    }
}