using UnityEngine;

/// <summary>
/// 1. 맵 데이터 생성
/// 2. 청크 생성
/// 3. 각 청크가 자기 구역 타일 생성
/// 4. 각 객체의 타일 위치 관리
/// 5. Building / Tree 등 MapObject 점유 관리
/// </summary>
[ExecuteAlways]
public class MapManager : MonoBehaviour
{
    [Header("<< 맵 사이즈 >>")]
    [SerializeField] private int mapWidth = 128;
    [SerializeField] private int mapHeight = 128;
    [SerializeField] private int chunkSize = 16;

    [Header("<< 맵 프리팹 >>")]
    [SerializeField] private GameObject grassPrefab;

    private TileData[,] mapData;

    private void Start()
    {
        if (Application.isPlaying)
        {
            GenerateMap();
        }
    }

    #region < ContextMenu >

    [ContextMenu("Generate Map")]
    private void GenerateMap()
    {
        ClearMap();
        GenerateMapData();
        RegisterInitialMapObjects();
        GenerateChunks();
    }

    [ContextMenu("Clear Map")]
    private void ClearMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    #endregion

    #region < Map 생성 >

    private void GenerateMapData()
    {
        mapData = new TileData[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                mapData[x, z] = new TileData
                {
                    x = x,
                    z = z,
                    height = 1,
                    tileType = TileType.Grass,
                    buildable = true,
                    occupied = false
                };
            }
        }
    }



    private void GenerateChunks()
    {
        int chunkCountX = mapWidth / chunkSize;
        int chunkCountZ = mapHeight / chunkSize;

        for (int ccx = 0; ccx < chunkCountX; ccx++)
        {
            for (int ccz = 0; ccz < chunkCountZ; ccz++)
            {
                GameObject chunkObject = new GameObject($"Chunk_{ccx}_{ccz}");
                chunkObject.transform.parent = transform;

                Chunk chunk = chunkObject.AddComponent<Chunk>();
                chunk.Init(ccx, ccz, chunkSize, mapData, grassPrefab);
                chunk.GenerateChunk();
            }
        }
    }

    #endregion

    #region < Tile / Bound >

    public Vector2Int WorldToTile(Vector3 worldPos)
    {
        int tileX = Mathf.RoundToInt(worldPos.x + mapWidth / 2f);
        int tileZ = Mathf.RoundToInt(worldPos.z + mapHeight / 2f);

        return new Vector2Int(tileX, tileZ);
    }

    public Vector3 TileToWorld(Vector2Int tilePos)
    {
        float worldX = tilePos.x - mapWidth / 2f;
        float worldZ = tilePos.y - mapHeight / 2f;

        return new Vector3(worldX, 0f, worldZ);
    }

    public bool IsWithinMapBounds(int x, int z)
    {
        return x >= 0 && x < mapWidth &&
               z >= 0 && z < mapHeight;
    }

    #endregion

    #region < 초기 MapObject 점유상태 >

    private void RegisterInitialMapObjects()
    {
        MapObject[] mapObjects = FindObjectsByType<MapObject>(FindObjectsSortMode.None);

        foreach (MapObject mapObject in mapObjects)
        {
            SetObjectTilesOccupied(mapObject, true);
        }
    }

    public void SetObjectTilesOccupied(MapObject mapObject, bool occupied)
    {
        Vector2Int origin = WorldToTile(mapObject.transform.position);

        for (int x = 0; x < mapObject.Width; x++)
        {
            for (int z = 0; z < mapObject.Height; z++)
            {
                int tileX = origin.x + x;
                int tileZ = origin.y + z;

                if (IsWithinMapBounds(tileX, tileZ))
                {
                    mapData[tileX, tileZ].occupied = occupied;
                    mapData[tileX, tileZ].buildable = !occupied;
                }
            }
        }
    }

    #endregion

    #region < Move >

    public bool CanPlaceObject(MapObject mapObject, Vector2Int origin)
    {
        for (int x = 0; x < mapObject.Width; x++)
        {
            for (int z = 0; z < mapObject.Height; z++)
            {
                int tileX = origin.x + x;
                int tileZ = origin.y + z;

                if (!IsWithinMapBounds(tileX, tileZ))
                    return false;

                TileData tile = mapData[tileX, tileZ];

                if (!tile.buildable || tile.occupied)
                    return false;
            }
        }

        return true;
    }

    public void MoveObject(MapObject mapObject, Vector2Int newOrigin)
    {
        SetObjectTilesOccupied(mapObject, false);

        mapObject.transform.position = TileToWorld(newOrigin);

        SetObjectTilesOccupied(mapObject, true);
    }

    #endregion
}