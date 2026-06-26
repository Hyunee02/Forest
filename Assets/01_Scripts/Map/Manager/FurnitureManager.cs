using System.Collections.Generic;
using UnityEngine;

public class FurnitureManager : MonoBehaviour
{
    [Header("<< 가구 부모 오브젝트 >>")]
    [SerializeField] private Transform furnitureRoot;

    [Header("<< 셀 크기 >>")]
    [SerializeField] private float cellSize = 1f;

    [Header("<< 가구 프리팹 >>")]
    [SerializeField] private GameObject bedPrefab;
    [SerializeField] private GameObject chairPrefab;
    [SerializeField] private GameObject tablePrefab;

    private readonly List<GameObject> spawnedFurnitureList = new List<GameObject>();

    private void Start()
    {
        LoadFurniture();
    }

    private void LoadFurniture()
    {
        List<FurnitureSaveData> saveDataList = GetTestSaveData();

        foreach (FurnitureSaveData data in saveDataList)
        {
            SpawnFurniture(data);
        }
    }

    private void SpawnFurniture(FurnitureSaveData data)
    {
        GameObject prefab = GetFurniturePrefab(data.furnitureId);

        if (prefab == null)
        {
            Debug.LogWarning($"{data.furnitureId} 프리팹을 찾을 수 없습니다.");
            return;
        }

        Vector3 localPosition = GridToLocalPosition(data.gridPosition);

        GameObject furnitureObj = Instantiate(
            prefab,
            furnitureRoot
        );

        furnitureObj.transform.localPosition = localPosition;
        furnitureObj.transform.localRotation = Quaternion.Euler(0f, data.rotationY, 0f);

        FurnitureItem furnitureItem = furnitureObj.GetComponent<FurnitureItem>();

        if (furnitureItem != null)
        {
            furnitureItem.LoadFromSaveData(data);
        }

        spawnedFurnitureList.Add(furnitureObj);
    }

    private Vector3 GridToLocalPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * cellSize,
            0f,
            gridPosition.y * cellSize
        );
    }

    private GameObject GetFurniturePrefab(string furnitureId)
    {
        switch (furnitureId)
        {
            case "Bed_01":
                return bedPrefab;

            case "Chair_01":
                return chairPrefab;

            case "Table_01":
                return tablePrefab;

            default:
                return null;
        }
    }

    private List<FurnitureSaveData> GetTestSaveData()
    {
        return new List<FurnitureSaveData>
        {
            new FurnitureSaveData
            {
                furnitureId = "Bed_01",
                gridPosition = new Vector2Int(2, 3),
                rotationY = 0
            },

            new FurnitureSaveData
            {
                furnitureId = "Chair_01",
                gridPosition = new Vector2Int(4, 2),
                rotationY = 90
            }
        };
    }
}