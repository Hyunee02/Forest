using UnityEngine;

public class Chunk : MonoBehaviour
{
    private int chunkX;
    private int chunkZ;
    private int chunkSize;

    private TileData[,] mapData;
    private GameObject tilePrefab;

    public void Init(
        int chunkX,
        int chunkZ,
        int chunkSize,
        TileData[,] mapData,
        GameObject tilePrefab)
    {
        this.chunkX = chunkX;
        this.chunkZ = chunkZ;
        this.chunkSize = chunkSize;
        this.mapData = mapData;
        this.tilePrefab = tilePrefab;
    }

    public void GenerateChunk()
    {
        if (mapData == null || tilePrefab == null)
            return;

        int startX = chunkX * chunkSize;
        int startZ = chunkZ * chunkSize;

        float offsetX = mapData.GetLength(0) / 2f;
        float offsetZ = mapData.GetLength(1) / 2f;

        for (int x = startX; x < startX + chunkSize; x++)
        {
            for (int z = startZ; z < startZ + chunkSize; z++)
            {
                if (x >= mapData.GetLength(0) || z >= mapData.GetLength(1))
                    continue;

                TileData tileData = mapData[x, z];

                if (!tileData.active)
                    continue;

                Vector3 spawnPos = new Vector3(x - offsetX, 0f, z - offsetZ);
                Instantiate(tilePrefab, spawnPos, Quaternion.identity, transform);
            }
        }
    }
}