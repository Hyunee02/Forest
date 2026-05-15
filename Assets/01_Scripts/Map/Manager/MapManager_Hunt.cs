using UnityEngine;

public class MapManager_Hunt : MonoBehaviour
{
    [Header("<< ¸Ê »çÀÌÁî >>")]
    [SerializeField] private int mapWidth = 128;
    [SerializeField] private int mapHeight = 96;
    [SerializeField] private int chunkSize = 16;

    [Header("<< ¸Ê ÇÁ¸®ÆÕ >>")]
    [SerializeField] private GameObject sandPrefab;

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

        float radiusX = mapWidth * 0.48f;
        float radiusZ = mapHeight * 0.38f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                float nx = (x - center.x) / radiusX;
                float nz = (z - center.y) / radiusZ;

                float baseShape = nx * nx + nz * nz;

                float noise1 = Mathf.PerlinNoise(x * 0.06f, z * 0.06f);
                float noise2 = Mathf.PerlinNoise(x * 0.13f + 100f, z * 0.13f + 100f);

                float edgeNoise = (noise1 - 0.5f) * 0.45f + (noise2 - 0.5f) * 0.2f;

                bool isInside = baseShape < 1f + edgeNoise;

                mapData[x, z] = new TileData
                {
                    x = x,
                    z = z,
                    height = 1,
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
                GameObject chunkObject = new GameObject($"Chunk_{ccx}_{ccz}");
                chunkObject.transform.parent = transform;

                Chunk chunk = chunkObject.AddComponent<Chunk>();
                chunk.Init(ccx, ccz, chunkSize, mapData, sandPrefab);
                chunk.GenerateChunk();
            }
        }
    }

    #endregion
}
