using UnityEngine;

public class HouseInteriorManager : MonoBehaviour
{
    [Header("<< 내부 프리팹 >>")]
    [SerializeField] private GameObject Lv2Prefab;
    [SerializeField] private GameObject Lv3Prefab;
    [SerializeField] private GameObject Lv4Prefab;

    [Header("<< 생성 위치 >>")]
    [SerializeField] private Transform rootPoint;

    private GameObject currentInterior;

    private void Start()
    {
        LoadInterior();
    }

    private void LoadInterior()
    {
        int houseLevel = HouseUpgradeManager.Instance.HouseLevel;

        GameObject prefab = null;

        switch (houseLevel)
        {
            case 1:
                prefab = Lv2Prefab;
                break;

            case 2:
                prefab = Lv3Prefab;
                break;

            case 3:
                prefab = Lv4Prefab;
                break;
        }

        if (prefab == null)
        {
            Debug.LogWarning("집 내부 프리팹이 없습니다.");
            return;
        }

        if (currentInterior != null)
        {
            Destroy(currentInterior);
        }

        currentInterior = Instantiate(
            prefab,
            rootPoint.position,
            Quaternion.identity,
            rootPoint
        );
    }
}
