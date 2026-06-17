using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    private enum DungeonState
    {
        Idle,
        Stage1Wave1,
        Stage1Wave2,
        Stage2Wave1,
        Stage2Wave2,
        Boss,
        Cleared
    }

    [Header("<< 풀링 매니저 >>")]
    [SerializeField] private EnemyPoolManager enemyPool;

    [Header("<< 1단계 적 프리팹 >>")]
    [SerializeField] private DungeonEnemy[] stage1EnemyPrefabs;

    [Header("<< 1단계 스폰 포인트 >>")]
    [SerializeField] private Transform[] stage1Wave1SpawnPoints;
    [SerializeField] private Transform[] stage1Wave2SpawnPoints;

    [Header("<< 1단계 클리어 문 >>")]
    [SerializeField] private AutoDoor_Dungeon door1;

    [Header("<< 2단계 적 프리팹 >>")]
    [SerializeField] private DungeonEnemy[] stage2EnemyPrefabs;

    [Header("<< 2단계 스폰 포인트 >>")]
    [SerializeField] private Transform[] stage2Wave1SpawnPoints;
    [SerializeField] private Transform[] stage2Wave2SpawnPoints;

    [Header("<< 2단계 클리어 문 >>")]
    [SerializeField] private AutoDoor_Dungeon door2;

    [Header("<< 보스 프리팹 >>")]
    [SerializeField] private DungeonEnemy[] bossPrefabs;

    [Header("<< 보스 스폰 포인트 >>")]
    [SerializeField] private Transform bossSpawnPoint;

    [Header("<< 클리어 후 플레이어 이동 위치 >>")]
    [SerializeField] private Vector3 playerRespawnPosition = new Vector3(-16.25f, 0.5f, 20f);
    [SerializeField] private Vector3 playerRespawnEuler = new Vector3(0f, 180f, 0f);

    private DungeonState currentState = DungeonState.Idle;

    private int aliveEnemyCount;
    private readonly List<DungeonEnemy> activeEnemies = new();

    private Transform player;

    private void Awake()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    [ContextMenu("Start Dungeon Run")]
    public void StartDungeonRun()
    {
        Debug.Log("던전 시작");

        ResetDungeonRun();

        StartStage1Wave1();
    }

    [ContextMenu("Clear Dungeon Enemies")]
    private void ClearDungeonEnemies()
    {
        ReleaseAllActiveEnemies();

        aliveEnemyCount = 0;
        currentState = DungeonState.Idle;

        Debug.Log("던전 적들을 모두 정리했습니다.");
    }

    private void ResetDungeonRun()
    {
        ReleaseAllActiveEnemies();

        aliveEnemyCount = 0;
        currentState = DungeonState.Idle;

        if (door1 != null)
            door1.CloseInstant();

        if (door2 != null)
            door2.CloseInstant();
    }

    private void ReleaseAllActiveEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            DungeonEnemy enemy = activeEnemies[i];

            if (enemy != null && enemyPool != null)
            {
                enemyPool.Release(enemy);
            }
        }

        activeEnemies.Clear();
    }

    private void StartStage1Wave1()
    {
        currentState = DungeonState.Stage1Wave1;

        SpawnSameRandomEnemyWave(stage1EnemyPrefabs, stage1Wave1SpawnPoints);

        Debug.Log("1단계 1웨이브 시작");
    }

    private void StartStage1Wave2()
    {
        currentState = DungeonState.Stage1Wave2;

        SpawnSameRandomEnemyWave(stage1EnemyPrefabs, stage1Wave2SpawnPoints);

        Debug.Log("1단계 2웨이브 시작");
    }

    private void StartStage2Wave1()
    {
        currentState = DungeonState.Stage2Wave1;

        SpawnSameRandomEnemyWave(stage2EnemyPrefabs, stage2Wave1SpawnPoints);

        Debug.Log("2단계 1웨이브 시작");
    }

    private void StartStage2Wave2()
    {
        currentState = DungeonState.Stage2Wave2;

        SpawnSameRandomEnemyWave(stage2EnemyPrefabs, stage2Wave2SpawnPoints);

        Debug.Log("2단계 2웨이브 시작");
    }

    private void StartBossRoom()
    {
        currentState = DungeonState.Boss;

        SpawnBoss();

        Debug.Log("보스방 시작");
    }

    private void SpawnSameRandomEnemyWave(DungeonEnemy[] enemyPrefabs, Transform[] spawnPoints)
    {
        if (enemyPool == null)
        {
            Debug.LogWarning("DungeonEnemyPool이 연결되지 않았습니다.");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("적 프리팹 배열이 비어 있습니다.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("스폰 포인트 배열이 비어 있습니다.");
            return;
        }

        DungeonEnemy selectedPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        aliveEnemyCount = spawnPoints.Length;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
                continue;

            DungeonEnemy enemy = enemyPool.Spawn(
                selectedPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            if (enemy == null)
                continue;

            enemy.OnDied = HandleEnemyDied;
            activeEnemies.Add(enemy);
        }
    }

    private void SpawnBoss()
    {
        if (enemyPool == null)
        {
            Debug.LogWarning("DungeonEnemyPool이 연결되지 않았습니다.");
            return;
        }

        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            Debug.LogWarning("보스 프리팹 배열이 비어 있습니다.");
            return;
        }

        if (bossSpawnPoint == null)
        {
            Debug.LogWarning("보스 스폰 포인트가 없습니다.");
            return;
        }

        DungeonEnemy selectedBoss = bossPrefabs[Random.Range(0, bossPrefabs.Length)];

        aliveEnemyCount = 1;

        DungeonEnemy boss = enemyPool.Spawn(
            selectedBoss,
            bossSpawnPoint.position,
            bossSpawnPoint.rotation
        );

        if (boss == null)
            return;

        boss.OnDied = HandleEnemyDied;
        activeEnemies.Add(boss);
    }

    private void HandleEnemyDied(DungeonEnemy enemy)
    {
        if (enemy == null)
            return;

        enemy.OnDied = null;

        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);

        aliveEnemyCount--;

        if (enemyPool != null)
            enemyPool.Release(enemy);

        Debug.Log($"남은 적 수: {aliveEnemyCount}");

        if (aliveEnemyCount <= 0)
        {
            HandleWaveCleared();
        }
    }

    private void HandleWaveCleared()
    {
        switch (currentState)
        {
            case DungeonState.Stage1Wave1:
                StartStage1Wave2();
                break;

            case DungeonState.Stage1Wave2:
                Debug.Log("1단계 방 클리어");

                if (door1 != null)
                    door1.OpenDoor();

                StartStage2Wave1();
                break;

            case DungeonState.Stage2Wave1:
                StartStage2Wave2();
                break;

            case DungeonState.Stage2Wave2:
                Debug.Log("2단계 방 클리어");

                if (door2 != null)
                    door2.OpenDoor();

                StartBossRoom();
                break;

            case DungeonState.Boss:
                Debug.Log("던전 클리어");
                currentState = DungeonState.Cleared;
                TeleportPlayerAfterClear();
                break;
        }
    }

    private void TeleportPlayerAfterClear()
    {
        if (player == null)
            FindPlayer();

        if (player == null)
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            return;
        }

        player.SetPositionAndRotation(
            playerRespawnPosition,
            Quaternion.Euler(playerRespawnEuler)
        );

        Debug.Log("플레이어를 던전 밖으로 이동시켰습니다.");
    }
}