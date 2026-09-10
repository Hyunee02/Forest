using UnityEngine;

public class FurnitureItem : MonoBehaviour
{
    [Header("<< 가구 정보 >>")]
    [SerializeField] private string furnitureId;

    [Header("<< 가구 크기 >>")]
    [SerializeField] private int width = 1;
    [SerializeField] private int height = 1;

    [Header("<< 현재 그리드 위치 >>")]
    [SerializeField] private Vector2Int gridPosition;

    public string FurnitureId => furnitureId;
    public int Width => width;
    public int Height => height;
    public Vector2Int GridPosition => gridPosition;

    public void SetGridPosition(Vector2Int newGridPosition)
    {
        gridPosition = newGridPosition;
    }

    public FurnitureSaveData ToSaveData()
    {
        return new FurnitureSaveData
        {
            furnitureId = furnitureId,
            gridPosition = gridPosition,
            rotationY = Mathf.RoundToInt(transform.eulerAngles.y)
        };
    }

    public void LoadFromSaveData(FurnitureSaveData data)
    {
        furnitureId = data.furnitureId;
        gridPosition = data.gridPosition;
        transform.rotation = Quaternion.Euler(0f, data.rotationY, 0f);
    }
}