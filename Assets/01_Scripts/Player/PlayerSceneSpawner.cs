using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class PlayerSceneSpawner : MonoBehaviour
{
    [SerializeField] private CharacterDatabase database;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private string mapSceneName = "Map";

    [Header("Player 씬 단독 테스트용")]
    [SerializeField] private CharacterData testCharacter;

    private bool playerReady;

    private void Awake()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("PlayerSceneSpawner에 Spawn Point를 연결해주세요.", this);
            return;
        }

        CharacterData character = GetCharacterToSpawn();

        if (character == null || character.PlayerPrefab == null)
        {
            Debug.LogError(
                "생성할 플레이어를 찾지 못했어요. 선택한 ID와 Player Prefab을 확인해주세요.",
                this);
            return;
        }

        GameObject player = Instantiate(
            character.PlayerPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        // 에디터에서 Player 씬만 실행한 경우 테스트 캐릭터로 플레이한다.
        // 이 경우 실제 저장 파일을 읽거나 쓰지 않는다.
        if (GameSession.Profile != null)
        {
            if (!player.TryGetComponent(out PlayerSave save))
            {
                FailSpawn(player, "Player Prefab 루트에 PlayerSave를 추가해주세요.");
                return;
            }

            if (!save.TryPrepare(
                GameSession.Profile,
                GameSession.CurrentData,
                out string error))
            {
                FailSpawn(player, error);
                return;
            }
        }

        playerReady = true;
    }

    private void FailSpawn(GameObject player, string message)
    {
        Debug.LogError(message, this);
        player.SetActive(false);
        Destroy(player);
    }

    private CharacterData GetCharacterToSpawn()
    {
        CharacterCreateDraft profile = GameSession.Profile;

        if (profile != null)
        {
            if (database == null)
            {
                Debug.LogError("Character Database를 연결해주세요.", this);
                return null;
            }

            return database.GetCharacter(profile.characterId);
        }

#if UNITY_EDITOR
        return testCharacter;
#else
        return null;
#endif
    }

    private IEnumerator Start()
    {
        if (!playerReady)
            yield break;

        if (SceneManager.GetSceneByName(mapSceneName).isLoaded)
            yield break;

        if (!Application.CanStreamedLevelBeLoaded(mapSceneName))
        {
            Debug.LogError($"씬 목록에서 '{mapSceneName}'을 확인해주세요.", this);
            yield break;
        }

        yield return SceneManager.LoadSceneAsync(
            mapSceneName,
            LoadSceneMode.Additive);
    }
}