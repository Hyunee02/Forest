using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveStartUI : MonoBehaviour
{
    [Header("캐릭터 데이터")]
    [SerializeField] private CharacterDatabase database;

    [Header("타이틀 버튼")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("씬")]
    [SerializeField] private string characterSceneName = "Character";
    [SerializeField] private string playerSceneName = "Player";

    private bool loading;

    private void OnEnable()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);
        else
            Debug.LogWarning("StartMenu에 시작하기 버튼을 연결해주세요.", this);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
        else
            Debug.LogWarning("StartMenu에 이어하기 버튼을 연결해주세요.", this);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        else
            Debug.LogWarning("StartMenu에 종료하기 버튼을 연결해주세요.", this);
    }

    private void OnDisable()
    {
        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(NewGame);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueGame);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);
    }

    // 시작하기 버튼
    public void NewGame()
    {
        if (loading)
            return;

        if (GameSaveFile.Exists)
        {
            Debug.LogWarning("이미 주민 기록이 있어요. 이어하기를 눌러주세요.", this);
            return;
        }

        if (!CanOpen(characterSceneName))
            return;

        GameSession.Clear();
        Open(characterSceneName);
    }

    // 이어하기 버튼
    public void ContinueGame()
    {
        if (loading)
            return;

        if (!GameSaveFile.TryLoad(out GameSaveData data, out string error))
        {
            Debug.LogWarning(error, this);
            return;
        }

        if (database == null)
        {
            Debug.LogWarning("StartMenu에 Character Database를 연결해주세요.", this);
            return;
        }

        CharacterData character = database.GetCharacter(data.profile.characterId);

        if (character == null || character.PlayerPrefab == null)
        {
            Debug.LogWarning(
                $"저장된 캐릭터의 Player Prefab을 찾을 수 없어요. ID: {data.profile.characterId}",
                this);
            return;
        }

        if (!CanOpen(playerSceneName))
            return;

        GameSession.SetData(data);
        Open(playerSceneName);
    }

    // 종료하기 버튼
    public void QuitGame()
    {
        if (loading)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private bool CanOpen(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            return true;

        Debug.LogWarning($"씬 목록에서 '{sceneName}'을 확인해주세요.", this);
        return false;
    }

    private void Open(string sceneName)
    {
        loading = true;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}