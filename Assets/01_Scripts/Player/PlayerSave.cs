using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerSave : MonoBehaviour
{
    [Header("임시 테스트: 파일 로드/저장 생략")]
    [SerializeField] private bool temporaryTestMode = true;

    [Header("Map 씬 단독 테스트")]
    [SerializeField] private bool createTestDataIfMissing;
    [SerializeField] private string testCharacterId;
    [SerializeField, Min(0)] private int testMoney = 10000;

    [Header("선택 사항: 저장 상태 표시")]
    [SerializeField] private TMP_Text statusText;

    private PlayerInventory inventory;

    // 저장해야 할 변경 사항이 존재
    private bool dirty;
    // 현재 저장 데이터 복원 중
    private bool restoring;

    public bool IsReady { get; private set; }

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        inventory.OnChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        inventory.OnChanged -= OnInventoryChanged;

        if (IsReady && dirty)
            SaveNow();
    }

    private void Start()
    {
        if (temporaryTestMode)
        {
            GameSaveData testDatatwo = new()
            {
                money = Mathf.Max(0, testMoney),
                day = 1,
                tradeDay = 1,
                inventory = inventory.CaptureSaveItems()
            };

            GameSession.Instance.SetData(testDatatwo);

            // 현재 인벤토리를 그대로 사용하므로 복원은 생략
            IsReady = true;
            dirty = false;

            ShowStatus("임시 테스트 모드: 파일 저장 없이 시작합니다.");
            return;
        }

        // GameSession 데이터 확인
        GameSaveData data = GameSession.Instance.CurrentData;

        if (data != null)
        {
            GameSaveFile.UpgradeLegacyData(data, inventory);

            // 새 캐릭터 데이터에는 아직 슬롯 정보가 없음
            if (data.inventory == null)
                data.inventory = inventory.CaptureSaveItems();

            if (!ApplyData(data, out string error))
            {
                ShowStatus(error);
                return;
            }

            dirty = true;
            return;
        }

        if (GameSaveFile.Exists)
        {
            LoadNow();
            return;
        }

        if (!createTestDataIfMissing)
        {
            ShowStatus(
                "플레이어 데이터가 없습니다. 캐릭터 생성 후 진입해주세요.");

            return;
        }

        GameSaveData testData = new()
        {
            money = testMoney,

            profile = new PlayerProfileData
            {
                characterId = testCharacterId,
                playerName = "테스트"
            },

            inventory = inventory.CaptureSaveItems()
        };

        if (!ApplyData(testData, out string testError))
        {
            ShowStatus(testError);
            return;
        }

        dirty = true;
    }

    private void LateUpdate()
    {
        if (!IsReady || !dirty)
            return;

        // 실패 시 매 프레임 저장을 반복하지 않음
        dirty = false;

        if (!TrySave(out string error))
            ShowStatus(error);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && IsReady)
            SaveNow();
    }

    private void OnApplicationQuit()
    {
        if (IsReady)
            SaveNow();
    }

    private void OnInventoryChanged()
    {
        if (!IsReady || restoring)
            return;

        GameSession.Instance.CurrentData.inventory =
            inventory.CaptureSaveItems();

        dirty = true;
    }

    /// <summary>
    /// 저장 데이터 적용
    /// </summary>
    /// <param name="data"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private bool ApplyData(
        GameSaveData data,
        out string error)
    {
        if (!GameSaveFile.Validate(
                data,
                inventory,
                out error))
        {
            return false;
        }

        restoring = true;

        try
        {
            // 복원 이벤트에서 새 돈·날짜를 읽도록 먼저 설정
            GameSession.Instance.SetData(data);

            if (!inventory.RestoreSaveItems(
                    data.inventory,
                    out error))
            {
                return false;
            }

            IsReady = true;
            dirty = false;

            return true;
        }
        finally
        {
            restoring = false;
        }
    }

    /// <summary>
    /// 실제 저장 실행
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool TrySave(out string error)
    {
        error = "";

        if (temporaryTestMode)
        {
            dirty = false;
            return true;
        }

        if (!IsReady)
        {
            error = "플레이어 데이터가 아직 준비되지 않았습니다.";
            return false;
        }

        GameSaveData data = GameSession.Instance.CurrentData;

        data.inventory = inventory.CaptureSaveItems();

        if (!GameSaveFile.Validate(
                data,
                inventory,
                out error))
        {
            return false;
        }

        dirty = false;

        return GameSaveFile.TrySave(data, out error);
    }

    [ContextMenu("저장/현재 게임 저장")]
    public void SaveNow()
    {
        ShowStatus(
            TrySave(out string error)
                ? "저장했습니다."
                : error);
    }

    [ContextMenu("저장/게임 불러오기")]
    public void LoadNow()
    {
        if (temporaryTestMode)
        {
            ShowStatus("임시 테스트 모드에서는 파일을 불러오지 않습니다.");
            return;
        }

        if (IsGameplayBusy())
        {
            ShowStatus("대화나 도구 사용을 마친 뒤 불러와주세요.");
            return;
        }

        if (!GameSaveFile.TryLoad(
                inventory,
                out GameSaveData data,
                out string message))
        {
            ShowStatus(message);
            return;
        }

        if (!ApplyData(data, out string error))
        {
            ShowStatus(error);
            return;
        }

        ShowStatus(message);
    }

    [ContextMenu("날짜/다음 날")]
    public void NextDay()
    {
        if (!IsReady || IsGameplayBusy())
        {
            ShowStatus("현재는 날짜를 변경할 수 없습니다.");
            return;
        }

        GameSaveData data = GameSession.Instance.CurrentData;

        if (data.day == int.MaxValue)
        {
            ShowStatus("게임 날짜의 최대값에 도달했습니다.");
            return;
        }

        data.day++;
        data.tradeDay = data.day;
        data.dailyTrades.Clear();

        bool saved = TrySave(out string error);

        ShowStatus(
            saved
                ? $"{data.day}일 차가 되었습니다."
                : $"{data.day}일 차가 되었습니다.\n{error}");
    }

    private bool IsGameplayBusy()
    {
        if (NPCManager.Instance != null &&
            NPCManager.Instance.IsUIBusy)
        {
            return true;
        }

        PlayerEquip equip = GetComponent<PlayerEquip>();

        return equip != null && equip.BUse;
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log($"[PlayerSave] {message}", this);
    }
}