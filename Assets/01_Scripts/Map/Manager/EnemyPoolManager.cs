using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolInfo
    {
        public DungeonEnemy prefab;
        public int initialSize = 8;
    }

    [Header("<< 풀링 대상 >>")]
    [SerializeField] private PoolInfo[] poolInfos;

    private readonly Dictionary<DungeonEnemy, Queue<DungeonEnemy>> poolDict = new();
    private readonly Dictionary<DungeonEnemy, DungeonEnemy> instanceToPrefabDict = new();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        foreach (PoolInfo info in poolInfos)
        {
            if (info.prefab == null)
                continue;

            if (!poolDict.ContainsKey(info.prefab))
                poolDict.Add(info.prefab, new Queue<DungeonEnemy>());

            for (int i = 0; i < info.initialSize; i++)
            {
                DungeonEnemy enemy = CreateEnemy(info.prefab);
                poolDict[info.prefab].Enqueue(enemy);
            }
        }
    }

    private DungeonEnemy CreateEnemy(DungeonEnemy prefab)
    {
        DungeonEnemy enemy = Instantiate(prefab, transform);
        enemy.gameObject.SetActive(false);

        instanceToPrefabDict[enemy] = prefab;

        return enemy;
    }

    public DungeonEnemy Spawn(DungeonEnemy prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        if (!poolDict.ContainsKey(prefab))
        {
            poolDict.Add(prefab, new Queue<DungeonEnemy>());
        }

        DungeonEnemy enemy;

        if (poolDict[prefab].Count > 0)
        {
            enemy = poolDict[prefab].Dequeue();
        }
        else
        {
            enemy = CreateEnemy(prefab);
        }

        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.gameObject.SetActive(true);
        enemy.ResetEnemy();

        return enemy;
    }

    public void Release(DungeonEnemy enemy)
    {
        if (enemy == null)
            return;

        enemy.OnDied = null;
        enemy.gameObject.SetActive(false);

        if (instanceToPrefabDict.TryGetValue(enemy, out DungeonEnemy prefab))
        {
            poolDict[prefab].Enqueue(enemy);
        }
        else
        {
            Destroy(enemy.gameObject);
        }
    }
}