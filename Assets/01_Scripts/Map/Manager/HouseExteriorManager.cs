using UnityEngine;

public class HouseExteriorManager : MonoBehaviour
{
    [Header("<< 외관 프리팹 >>")]
    [SerializeField] private GameObject tentExteriorPrefab;
    [SerializeField] private GameObject houseLevel1ExteriorPrefab;
    [SerializeField] private GameObject houseLevel2ExteriorPrefab;
    [SerializeField] private GameObject houseLevel3ExteriorPrefab;

    [Header("<< 집 생성 위치 >>")]
    [SerializeField] private Transform houseSpawnPoint;

    private GameObject currentHouseExterior;

    private void Start()
    {
        LoadExteriorByHouseLevel();
    }

    private void LoadExteriorByHouseLevel()
    {
        int houseLevel = HouseUpgradeManager.Instance.HouseLevel;

        GameObject prefabToSpawn = null;

        switch (houseLevel)
        {
            case 0:
                prefabToSpawn = tentExteriorPrefab;
                break;

            case 1:
                prefabToSpawn = houseLevel1ExteriorPrefab;
                break;

            case 2:
                prefabToSpawn = houseLevel2ExteriorPrefab;
                break;

            case 3:
                prefabToSpawn = houseLevel3ExteriorPrefab;
                break;
        }

        if (currentHouseExterior != null)
        {
            Destroy(currentHouseExterior);
        }

        currentHouseExterior = Instantiate(
            prefabToSpawn,
            houseSpawnPoint.position,
            Quaternion.identity
        );
    }
}