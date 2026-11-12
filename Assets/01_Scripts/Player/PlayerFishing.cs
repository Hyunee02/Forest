using TMPro;
using UnityEngine;

public enum FishingState
{
    Idle,
    Casting,
    Searching,
    Approaching,
    Waiting,
    Nibbling,
    Bite
}

[RequireComponent(typeof(PlayerEquip))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerFishing : MonoBehaviour
{
    [Header("물 판정")]
    [SerializeField] private LayerMask waterLayer;
    [SerializeField, Min(0.1f)] private float castDistance = 3f;
    [SerializeField, Min(0.1f)] private float rayHeight = 3f;
    [SerializeField, Min(0.1f)] private float rayDistance = 6f;

    [Header("물고기 탐색")]
    [SerializeField] private LayerMask fishLayer;
    [SerializeField, Min(0.1f)] private float searchRadius = 3f;
    [SerializeField, Min(0.05f)] private float searchInterval = 0.2f;
    [SerializeField, Min(1f)] private float approachTimeout = 10f;

    [Header("찌 던지기")]
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField, Min(0.05f)] private float castDuration = 0.5f;
    [SerializeField, Min(0f)] private float arcHeight = 1.2f;
    [SerializeField] private float surfaceOffset = 0.03f;

    [Header("입질 간격")]
    [SerializeField, Min(0.1f)] private float minWaitTime = 1f;
    [SerializeField, Min(0.1f)] private float maxWaitTime = 2.5f;

    [Header("가짜 입질")]
    [SerializeField, Range(0, 4)] private int maxFakeBites = 4;
    [SerializeField, Min(0.05f)] private float nibbleDuration = 0.2f;
    [SerializeField, Min(0f)] private float nibbleDepth = 0.06f;

    [Header("진짜 입질")]
    [SerializeField, Min(0.1f)] private float reactionTime = 0.8f;
    [SerializeField, Min(0f)] private float biteDepth = 0.25f;

    [Header("수면 움직임")]
    [SerializeField, Min(0f)] private float floatAmplitude = 0.02f;
    [SerializeField, Min(0f)] private float floatSpeed = 3f;

    [Header("표시")]
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject splashPrefab;

    [Header("효과음")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip nibbleSound;
    [SerializeField] private AudioClip biteSound;
    [SerializeField] private AudioClip catchSound;

    private PlayerEquip equip;
    private PlayerInventory inventory;
    private PlayerRest rest;

    private Tool_FishingRod rod;
    private FishingFish targetFish;
    private GameObject bobber;

    private Vector3 castStart;
    private Vector3 waterPoint;

    private int fakeBitesRemaining;

    private float stateStartedAt;
    private float stateEndsAt;
    private float nextSearchTime;
    private float nextCastTime;

    public FishingState State { get; private set; }

    public bool IsFishing => State != FishingState.Idle;

    private void Awake()
    {
        equip = GetComponent<PlayerEquip>();
        inventory = GetComponent<PlayerInventory>();
        rest = GetComponent<PlayerRest>();

        if (fishingLine != null)
        {
            fishingLine.useWorldSpace = true;
            fishingLine.positionCount = 2;
            fishingLine.enabled = false;
        }
    }

    private void OnEnable()
    {
        equip.OnEquipChanged += HandleEquipChanged;
    }

    private void OnDisable()
    {
        equip.OnEquipChanged -= HandleEquipChanged;
        AbortFishing();
    }

    private void Update()
    {
        if (!IsFishing)
            return;

        if (!IsSessionValid() || IsOtherActionBusy())
        {
            FinishFishing("낚시가 중단됐어요.", false);
            return;
        }

        if (State == FishingState.Casting)
        {
            UpdateCasting();
            return;
        }

        UpdateBobber();

        switch (State)
        {
            case FishingState.Searching:
                SearchFish();
                break;

            case FishingState.Approaching:
                UpdateApproaching();
                break;

            case FishingState.Waiting:
                if (Time.time >= stateEndsAt)
                    BeginNextBite();
                break;

            case FishingState.Nibbling:
                if (Time.time >= stateEndsAt)
                    BeginWaiting();
                break;

            case FishingState.Bite:
                if (Time.time >= stateEndsAt)
                    FinishFishing("물고기가 도망갔어요.", true);
                break;
        }
    }

    private void LateUpdate()
    {
        if (fishingLine == null)
            return;

        bool visible =
            IsFishing
            && rod != null
            && bobber != null;

        fishingLine.enabled = visible;

        if (!visible)
            return;

        fishingLine.SetPosition(0, rod.LineStart.position);
        fishingLine.SetPosition(1, bobber.transform.position);
    }

    /// <summary>
    /// PlayerEquip에서 도구 사용 입력을 전달
    /// </summary>
    public void HandleUse()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f)
            return;

        if (!IsFishing)
        {
            TryCast();
            return;
        }

        if (!IsSessionValid() || IsOtherActionBusy())
        {
            FinishFishing("낚시가 중단됐어요.", false);
            return;
        }

        // 찌가 날아가는 중에는 연속 입력 무시
        if (State == FishingState.Casting)
            return;

        if (State == FishingState.Bite)
        {
            // Update 실행 순서와 관계없이 시간 초과 검사
            if (Time.time >= stateEndsAt)
            {
                FinishFishing("물고기가 도망갔어요.", true);
                return;
            }

            CatchFish();
            return;
        }

        // 진짜 입질 전에 누르면 회수 또는 실패
        string message = targetFish != null
            ? "너무 일찍 당겼어요!"
            : "찌를 회수했어요.";

        FinishFishing(message, true);
    }

    private void TryCast()
    {
        if (Time.time < nextCastTime
            || equip.BUse
            || IsOtherActionBusy())
            return;

        Tool_FishingRod equippedRod =
            equip.CurTool as Tool_FishingRod;

        if (equippedRod == null
            || equippedRod.ToolData == null
            || equip.CurInventoryItem == null
            || equip.CurInventoryItem.currentDurability <= 0)
            return;

        if (bobberPrefab == null)
        {
            Debug.LogError("찌 프리팹을 연결해주세요.", this);
            return;
        }

        Vector3 origin =
            transform.position
            + transform.forward * castDistance
            + Vector3.up * rayHeight;

        if (!Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                rayDistance,
                waterLayer,
                QueryTriggerInteraction.Collide))
        {
            ShowStatus("앞쪽에 물이 없어요.");
            return;
        }

        rod = equippedRod;
        targetFish = null;

        castStart = rod.LineStart.position;
        waterPoint = hit.point + Vector3.up * surfaceOffset;

        bobber = Instantiate(
            bobberPrefab,
            castStart,
            Quaternion.identity);

        SetState(FishingState.Casting, castDuration);
        ShowStatus("");
    }

    private bool IsOtherActionBusy()
    {
        if (rest != null && rest.BRest)
            return true;

        return NPCManager.Instance != null
            && NPCManager.Instance.IsUIBusy;
    }

    private bool IsSessionValid()
    {
        if (rod == null
            || !rod.isActiveAndEnabled
            || rod.ToolData == null
            || equip == null
            || !equip.isActiveAndEnabled
            || equip.CurTool != rod
            || equip.CurInventoryItem == null
            || equip.CurInventoryItem.currentDurability <= 0
            || bobber == null)
            return false;

        bool needsFish =
            State == FishingState.Approaching
            || State == FishingState.Waiting
            || State == FishingState.Nibbling
            || State == FishingState.Bite;

        if (!needsFish)
            return true;

        return targetFish != null
            && targetFish.isActiveAndEnabled
            && targetFish.IsOwnedBy(this);
    }

    private void UpdateCasting()
    {
        float duration = stateEndsAt - stateStartedAt;

        float t = Mathf.Clamp01(
            (Time.time - stateStartedAt) / duration);

        Vector3 position =
            Vector3.Lerp(castStart, waterPoint, t);

        // 가운데에서 높아지는 포물선
        position.y += 4f * arcHeight * t * (1f - t);

        bobber.transform.position = position;

        if (t < 1f)
            return;

        SetState(FishingState.Searching, 0f);
        nextSearchTime = Time.time;
    }

    private void SearchFish()
    {
        if (Time.time < nextSearchTime)
            return;

        nextSearchTime =
            Time.time + Mathf.Max(0.05f, searchInterval);

        Collider[] hits = Physics.OverlapSphere(
            waterPoint,
            searchRadius,
            fishLayer,
            QueryTriggerInteraction.Collide);

        FishingFish closestFish = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            FishingFish fish =
                hit.GetComponentInParent<FishingFish>();

            if (fish == null || !fish.CanNotice(waterPoint))
                continue;

            float distance =
                (fish.transform.position - waterPoint).sqrMagnitude;

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestFish = fish;
        }

        if (closestFish == null
            || !closestFish.TryReserve(this))
            return;

        targetFish = closestFish;

        SetState(
            FishingState.Approaching,
            Mathf.Max(1f, approachTimeout));
    }

    private void UpdateApproaching()
    {
        if (targetFish.Approach(waterPoint))
        {
            fakeBitesRemaining = Random.Range(
                0,
                Mathf.Clamp(maxFakeBites, 0, 4) + 1);

            BeginWaiting();
            return;
        }

        if (Time.time >= stateEndsAt)
            FinishFishing("찌를 회수했어요.", false);
    }

    private void BeginWaiting()
    {
        float min = Mathf.Max(0.1f, minWaitTime);
        float max = Mathf.Max(min, maxWaitTime);

        SetState(
            FishingState.Waiting,
            Random.Range(min, max));

        ShowStatus("");
    }

    private void BeginNextBite()
    {
        if (fakeBitesRemaining > 0)
        {
            fakeBitesRemaining--;

            SetState(FishingState.Nibbling, nibbleDuration);
            PlaySound(nibbleSound);
            return;
        }

        SetState(FishingState.Bite, reactionTime);

        PlaySound(biteSound);
        SpawnSplash();

        ShowStatus("!");
    }

    private void UpdateBobber()
    {
        float height =
            Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        if (State == FishingState.Nibbling)
        {
            float duration = stateEndsAt - stateStartedAt;

            float t = Mathf.Clamp01(
                (Time.time - stateStartedAt) / duration);

            // 가짜 입질: 살짝 잠겼다가 올라옴
            height -= Mathf.Sin(t * Mathf.PI) * nibbleDepth;
        }
        else if (State == FishingState.Bite)
        {
            // 진짜 입질: 깊게 잠긴 상태 유지
            height = -biteDepth;
        }

        bobber.transform.position =
            waterPoint + Vector3.up * height;
    }

    private void CatchFish()
    {
        ItemData_SO fishData = targetFish.ItemData;

        ItemData_SO registeredData = fishData != null
            ? inventory.GetItemData(fishData.Id)
            : null;

        if (registeredData == null
            || registeredData.ItemType == ItemTypeSO.Tool
            || registeredData.ItemType == ItemTypeSO.None)
        {
            Debug.LogError(
                "물고기 ItemData와 ItemDatabase 등록을 확인해주세요.",
                targetFish);

            FinishFishing("물고기 정보를 확인할 수 없어요.", false);
            return;
        }

        if (!inventory.AddItem(registeredData.Id, 1))
        {
            FinishFishing(
                "가방이 가득 차서 물고기를 놓아줬어요.",
                false);
            return;
        }

        string fishName = registeredData.ItemName;
        int reduce = Mathf.Max(0, rod.ToolData.DurabilityReduce);

        targetFish.Caught(this);
        targetFish = null;

        PlaySound(catchSound);

        // 내구도 감소로 파손 이벤트가 발생하기 전에 상태 정리
        FinishFishing($"{fishName}을(를) 잡았어요!", false);

        equip.ReduceEquippedDurability(reduce);
    }

    /// <summary>
    /// 취소 버튼 등에 연결
    /// </summary>
    public void CancelFishing()
    {
        if (IsFishing)
            FinishFishing("찌를 회수했어요.", true);
    }

    /// <summary>
    /// 컴포넌트 비활성화 등의 강제 중단
    /// </summary>
    public void AbortFishing()
    {
        FinishFishing("", false);
    }

    private void HandleEquipChanged()
    {
        if (IsFishing && equip.CurTool != rod)
            FinishFishing("낚시가 중단됐어요.", false);
    }

    private void FinishFishing(string message, bool scareFish)
    {
        if (IsFishing)
        {
            float cooldown =
                rod != null && rod.ToolData != null
                    ? rod.ToolData.Cooldown
                    : 0f;

            nextCastTime =
                Time.time + Mathf.Max(0.2f, cooldown);
        }

        if (targetFish != null)
        {
            if (scareFish)
                targetFish.Escape(this, waterPoint);
            else
                targetFish.Release(this);
        }

        targetFish = null;
        State = FishingState.Idle;

        if (bobber != null)
            Destroy(bobber);

        bobber = null;
        rod = null;

        if (fishingLine != null)
            fishingLine.enabled = false;

        ShowStatus(message);
    }

    private void SetState(FishingState state, float duration)
    {
        State = state;
        stateStartedAt = Time.time;
        stateEndsAt = Time.time + Mathf.Max(0.01f, duration);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void SpawnSplash()
    {
        if (splashPrefab == null)
            return;

        GameObject splash = Instantiate(
            splashPrefab,
            waterPoint,
            Quaternion.identity);

        Destroy(splash, 2f);
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}