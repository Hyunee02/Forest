using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerSave : MonoBehaviour
{
    [Header("선택 사항: 저장 상태 표시")]
    [SerializeField] private TMP_Text statusText;

    private PlayerInventory inventory;
    private bool dirty;
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

    // LoadPlayerScene이 비활성 프리팹을 생성한 직후 호출한다.
    public bool TryInitialize(
        GameSaveData data,
        bool newGame,
        out string error)
    {
        inventory = GetComponent<PlayerInventory>();

        if (newGame && data.inventory == null)
            data.inventory = inventory.CaptureSaveItems();

        return ApplyData(data, out error);
    }

    private void LateUpdate()
    {
        if (!IsReady || !dirty)
            return;

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

        GameSession.Instance.CurrentData.inventory = inventory.CaptureSaveItems();
        dirty = true;
    }

    private bool ApplyData(GameSaveData data, out string error)
    {
        if (!GameSaveFile.Validate(data, inventory, out error))
            return false;

        GameSaveData previous = GameSession.Instance.CurrentData;
        restoring = true;

        try
        {
            // 인벤토리 복원 이벤트에서 새 골드와 날짜를 읽을 수 있게 한다.
            GameSession.Instance.SetData(data);

            if (!inventory.RestoreSaveItems(data.inventory, out error))
            {
                GameSession.Instance.SetData(previous);
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

    public bool TrySave(out string error)
    {
        error = "";

        if (!IsReady)
        {
            error = "플레이어 데이터가 아직 준비되지 않았습니다.";
            return false;
        }

        GameSaveData data = GameSession.Instance.CurrentData;
        data.inventory = inventory.CaptureSaveItems();

        if (!GameSaveFile.Validate(data, inventory, out error))
            return false;

        dirty = false;
        return GameSaveFile.TrySave(data, out error);
    }

    [ContextMenu("저장/현재 게임 저장")]
    public void SaveNow()
    {
        ShowStatus(TrySave(out string error) ? "저장했습니다." : error);
    }

    [ContextMenu("저장/게임 불러오기")]
    public void LoadNow()
    {
        if (IsGameplayBusy())
        {
            ShowStatus("대화나 도구 사용을 마친 뒤 불러와주세요.");
            return;
        }

        if (!GameSaveFile.TryLoad(
                inventory, out GameSaveData data, out string message))
        {
            ShowStatus(message);
            return;
        }

        if (!IsReady ||
            GameSession.Instance.CurrentData.profile.characterId != data.profile.characterId)
        {
            ShowStatus("다른 캐릭터의 저장은 시작 화면의 이어하기로 열어주세요.");
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

        ShowStatus(saved
            ? $"{data.day}일 차가 되었습니다."
            : $"{data.day}일 차가 되었습니다.\n{error}");
    }

    private bool IsGameplayBusy()
    {
        if (NPCManager.Instance != null && NPCManager.Instance.IsUIBusy)
            return true;

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