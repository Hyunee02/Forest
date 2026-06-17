using UnityEngine;

public class MapManager_Dungeon : MonoBehaviour
{
    [Header("<< ¸Ê »çÀÌÁî >>")]
    [SerializeField] private int mapWidth = 64;
    [SerializeField] private int mapHeight = 96;
    [SerializeField] private int chunkSize = 16;

    [Header("<< ¸Ê ÇÁ¸®ÆÕ >>")]
    [SerializeField] private GameObject dungeonPrefab;

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

    #region < Map »ý¼º >

    private void GenerateMapData()
    {
        mapData = new TileData[mapWidth, mapHeight];

        Vector2 center = new Vector2(mapWidth / 2f, mapHeight / 2f);

        float radiusX = mapWidth * 0.43f;
        float radiusZ = mapHeight * 0.43f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                float nx = (x - center.x) / radiusX;
                float nz = (z - center.y) / radiusZ;

                float baseShape = nx * nx + nz * nz;

                float noise1 = Mathf.PerlinNoise(x * 0.08f, z * 0.08f);
                float noise2 = Mathf.PerlinNoise(x * 0.17f + 200f, z * 0.17f + 200f);
                float noise3 = Mathf.PerlinNoise(x * 0.035f + 500f, z * 0.035f + 500f);

                float edgeNoise =
                    (noise1 - 0.5f) * 0.45f +
                    (noise2 - 0.5f) * 0.25f +
                    (noise3 - 0.5f) * 0.35f;

                bool isInside = baseShape < 1f + edgeNoise;

                mapData[x, z] = new TileData
                {
                    x = x,
                    z = z,
                    height = 1,

                    // TileType¿¡ DungeonÀÌ ÀÖÀ¸¸é DungeonÀ¸·Î ¹Ù²Ù¸é µÊ.
                    tileType = TileType.Sand,

                    active = isInside,
                    buildable = false,
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
                GameObject chunkObject = new GameObject($"Dungeon_Chunk_{ccx}_{ccz}");
                chunkObject.transform.parent = transform;

                Chunk chunk = chunkObject.AddComponent<Chunk>();
                chunk.Init(ccx, ccz, chunkSize, mapData, dungeonPrefab);
                chunk.GenerateChunk();
            }
        }
    }

    #endregion
}