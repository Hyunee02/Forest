using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPlayerScene : MonoBehaviour
{
    [SerializeField] private CharacterSelectUI selectUI;
    [SerializeField] private CharacterDatabase database;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField, Min(0)] private int startingMoney;

    [SerializeField] private string playerSceneName = "Player";
    [SerializeField] private string mapSceneName = "Map";

    [Tooltip("Character 씬 전용 카메라와 EventSystem을 묶은 오브젝트")]
    [SerializeField] private GameObject entrySystems;

    private Transform spawnPoint;
    private PlayerSave spawned;
    private bool loading;

    public void StartNewGame()
    {
        if (loading)
            return;

        if (selectUI == null || !selectUI.IsConfirmed || nameInput == null)
        {
            Show("캐릭터를 확정하고 이름을 입력해주세요.");
            return;
        }

        selectUI.Draft.playerName = nameInput.text;

        if (!selectUI.Draft.TryCreateSaveData(
                database, out GameSaveData data, out string error))
        {
            Show(error);
            return;
        }

        data.money = startingMoney;
        Begin(data);
    }

    public void ContinueGame()
    {
        if (loading)
            return;

        if (!GameSaveFile.Exists)
        {
            Show("이어할 저장 파일이 없습니다.");
            return;
        }

        Begin(null);
    }

    private void Begin(GameSaveData newGame)
    {
        if (database == null || !database.ValidateDatabase(out _))
        {
            Show("캐릭터 데이터베이스의 ID와 프리팹 연결을 확인해주세요.");
            return;
        }

        if (playerSceneName == mapSceneName ||
            !Application.CanStreamedLevelBeLoaded(playerSceneName) ||
            !Application.CanStreamedLevelBeLoaded(mapSceneName) ||
            SceneManager.GetSceneByName(playerSceneName).isLoaded ||
            SceneManager.GetSceneByName(mapSceneName).isLoaded)
        {
            Show("Character 씬만 열고, Build Profiles의 Player 및 Map 등록을 확인해주세요.");
            return;
        }

        loading = true;
        Show("섬으로 이동하고 있어요...");
        StartCoroutine(Enter(newGame));
    }

    private IEnumerator Enter(GameSaveData newGame)
    {
        Scene entry = gameObject.scene;

        yield return SceneManager.LoadSceneAsync(
            playerSceneName, LoadSceneMode.Additive);

        Scene playerScene = SceneManager.GetSceneByName(playerSceneName);

        foreach (GameObject root in playerScene.GetRootGameObjects())
        {
            if (root.name != "SpawnPoint")
                continue;

            // SpawnPoint가 둘 이상이면 어느 위치를 쓸지 확정할 수 없다.
            if (spawnPoint != null)
            {
                spawnPoint = null;
                break;
            }

            spawnPoint = root.transform;
        }

        string message = "Player 씬 최상위에 SpawnPoint 하나를 배치해주세요.";
        bool success = false;

        if (spawnPoint != null)
        {
            if (newGame != null)
                success = Spawn(newGame, true, out message);
            else
                success = GameSaveFile.TryLoad(SpawnSaved, out _, out message);
        }

        if (!success)
        {
            yield return SceneManager.UnloadSceneAsync(playerScene);

            spawnPoint = null;
            loading = false;
            Show(message);
            yield break;
        }

        AsyncOperation mapLoad = SceneManager.LoadSceneAsync(
            mapSceneName, LoadSceneMode.Additive);

        mapLoad.allowSceneActivation = false;

        while (mapLoad.progress < 0.9f)
            yield return null;

        if (entrySystems != null)
            entrySystems.SetActive(false);

        // Map의 카메라와 UI가 Awake에서 Player 태그를 찾기 전에 활성화한다.
        spawned.gameObject.SetActive(true);

        mapLoad.allowSceneActivation = true;
        yield return mapLoad;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mapSceneName));

        if (newGame != null)
            spawned.SaveNow();
        else
            Debug.Log(message, spawned);

        SceneManager.UnloadSceneAsync(entry);
    }

    private bool SpawnSaved(GameSaveData data, out string error)
    {
        return Spawn(data, false, out error);
    }

    private bool Spawn(GameSaveData data, bool newGame, out string error)
    {
        if (data == null || data.profile == null)
        {
            error = "저장된 프로필이 없습니다.";
            return false;
        }

        CharacterData character = database.GetCharacter(data.profile.characterId);

        if (character == null)
        {
            error = $"등록되지 않은 캐릭터 ID입니다. ({data.profile.characterId})";
            return false;
        }

        PlayerSave prefab = character.PlayerPrefab;

        if (prefab.transform.parent != null ||
            prefab.gameObject.activeSelf ||
            !prefab.enabled ||
            !prefab.CompareTag("Player"))
        {
            error = $"{character.ID}의 플레이 프리팹 루트에 Player 태그와 활성 PlayerSave를 설정한 뒤 루트 체크를 꺼주세요.";
            return false;
        }

        PlayerSave player = Instantiate(
            prefab, spawnPoint.position, spawnPoint.rotation);

        SceneManager.MoveGameObjectToScene(
            player.gameObject, spawnPoint.gameObject.scene);

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        GameSaveFile.UpgradeLegacyData(data, inventory);

        if (!CharacterNameRule.GameSaveValidator.Validate(data, database, out error) ||
            !player.TryInitialize(data, newGame, out error))
        {
            Destroy(player.gameObject);
            return false;
        }

        spawned = player;
        return true;
    }

    private void Show(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log($"[LoadPlayerScene] {message}", this);
    }
}