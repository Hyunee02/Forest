using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerEquip))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerFishing : MonoBehaviour
{
    [System.Serializable]
    private class FishEntry
    {
        public ItemData_SO item;

        [Range(0, 100)]
        public int weight = 1;
    }

    [Header("물 판정")]
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private float castDistance = 3f;
    [SerializeField] private float rayHeight = 3f;
    [SerializeField] private float rayDistance = 6f;

    [Header("찌")]
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField] private float surfaceOffset = 0.05f;
    [SerializeField] private float biteDepth = 0.2f;

    [Header("낚시 보상과 등장 가중치")]
    [SerializeField] private FishEntry[] fishTable = new FishEntry[0];

    // 이번에 던진 찌로 잡을 물고기
    private ItemData_SO fishItem;

    [Header("입질")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private float reactionTime = 1.5f;
    private bool isReeling;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    [Header("안내 - 선택 사항")]
    [SerializeField] private TMP_Text statusText;

    private PlayerEquip equip;
    private PlayerInventory inventory;
    private PlayerRest rest;

    private Tool_FishingRod rod;
    private GameObject bobber;
    private Vector3 waterPoint;

    private bool hasBite;
    private float biteTime;
    private float expireTime;
    private float nextCastTime;

    public bool IsFishing { get; private set; }

    private static readonly int FishingModeHash =
    Animator.StringToHash("FishingMode");

    private static readonly int UseToolHash =
    Animator.StringToHash("UseTool");

    private void Awake()
    {
        equip = GetComponent<PlayerEquip>();
        inventory = GetComponent<PlayerInventory>();
        rest = GetComponent<PlayerRest>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        equip.OnEquipChanged += CheckEquipment;
    }

    private void OnDisable()
    {
        equip.OnEquipChanged -= CheckEquipment;
        AbortFishing();
    }

    private void Update()
    {
        if (!IsFishing || isReeling)
            return;

        if (!CanContinue() || IsOtherActionBusy())
        {
            FinishFishing("낚시를 중단했어요.");
            return;
        }

        if (!hasBite)
        {
            if (Time.time >= biteTime)
            {
                hasBite = true;
                expireTime = Time.time + Mathf.Max(0.1f, reactionTime);

                // 입질이 오면 찌가 아래로 잠김
                bobber.transform.position =
                    waterPoint + Vector3.down * biteDepth;

                ShowStatus("입질! 지금 클릭!");
            }

            return;
        }

        if (Time.time >= expireTime)
            FinishFishing("물고기를 놓쳤어요.");
    }

    // 기존 PlayerEquip.UseTool()에서 호출
    public void HandleUse()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || isReeling)
            return;

        if (!IsFishing)
        {
            StartFishing();
            return;
        }

        if (!CanContinue() || IsOtherActionBusy())
        {
            FinishFishing("낚시를 중단했어요.");
            return;
        }

        // 입질 전 클릭은 무시하고 현재 찌와 입질 대기 시간을 유지한다.
        if (!hasBite)
            return;

        if (Time.time >= expireTime)
        {
            FinishFishing("물고기를 놓쳤어요.");
            return;
        }

        CatchFish();
    }

    private void StartFishing()
    {
        if (Time.time < nextCastTime
            || equip.BUse
            || IsOtherActionBusy())
            return;

        Tool_FishingRod currentRod =
            equip.CurTool as Tool_FishingRod;

        if (currentRod == null
            || currentRod.ToolData == null
            || equip.CurInventoryItem == null
            || equip.CurInventoryItem.currentDurability <= 0)
            return;

        if (bobberPrefab == null)
        {
            Debug.LogError("PlayerFishing의 Bobber Prefab을 연결해주세요.", this);
            return;
        }

        // 플레이어 앞쪽 지점에서 아래로 물 검사
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

        if (!TryRollFish(out ItemData_SO rolledFish))
            return;

        if (!inventory.CanAddItem(rolledFish.Id, 1))
        {
            ShowStatus("인벤토리에 잡은 물고기를 담을 공간이 없어요.");
            return;
        }

        fishItem = rolledFish;

        rod = currentRod;
        waterPoint = hit.point + Vector3.up * surfaceOffset;

        // 간단한 버전: 날아가는 연출 없이 수면에 바로 생성
        bobber = Instantiate(
            bobberPrefab,
            waterPoint,
            Quaternion.identity);

        float min = Mathf.Max(0.1f, minWaitTime);
        float max = Mathf.Max(min, maxWaitTime);

        biteTime = Time.time + Random.Range(min, max);
        hasBite = false;
        IsFishing = true;

        // 낚시 내부 상태: 대기
        animator.SetInteger(FishingModeHash, 1);

        // 도구 레이어에서 Player_Fishing으로 진입
        animator.ResetTrigger(UseToolHash);
        animator.SetTrigger(UseToolHash);

        ShowStatus("입질을 기다리는 중...");
    }

    private bool CanContinue()
    {
        return equip != null
            && equip.isActiveAndEnabled
            && rod != null
            && rod.isActiveAndEnabled
            && rod.ToolData != null
            && equip.CurTool == rod
            && equip.CurInventoryItem != null
            && equip.CurInventoryItem.currentDurability > 0
            && bobber != null
            && fishItem != null;
    }

    private bool IsOtherActionBusy()
    {
        if (rest != null && rest.BRest)
            return true;

        return NPCManager.Instance != null
            && NPCManager.Instance.IsUIBusy;
    }

    private void CatchFish()
    {
        int reduce = Mathf.Max(0, rod.ToolData.DurabilityReduce);
        string fishName = fishItem.ItemName;

        if (!inventory.AddItem(fishItem.Id, 1))
        {
            FinishFishing("인벤토리가 가득 찼어요.");
            return;
        }

        // IsFishing은 릴 애니메이션이 끝날 때까지 true로 유지한다.
        isReeling = true;
        hasBite = false;

        if (bobber != null)
            Destroy(bobber);

        bobber = null;

        ShowStatus($"{fishName} 획득!");
        SetFishingAnimation(2);

        equip.ReduceEquippedDurability(reduce);
    }

    public void CompleteReel()
    {
        if (!isReeling)
            return;

        FinishFishing("");
    }

    private void CheckEquipment()
    {
        if (IsFishing && !isReeling && equip.CurTool != rod)
            AbortFishing();
    }

    public void CancelFishing()
    {
        if (IsFishing && !isReeling)
            FinishFishing("찌를 회수했어요.");
    }

    public void AbortFishing()
    {
        if (IsFishing)
            FinishFishing("");
    }

    private void FinishFishing(string message)
    {
        float cooldown =
            rod != null && rod.ToolData != null
                ? rod.ToolData.Cooldown
                : 0f;

        nextCastTime = Time.time + Mathf.Max(0.2f, cooldown);

        IsFishing = false;
        isReeling = false;
        hasBite = false;

        SetFishingAnimation(0);

        if (bobber != null)
            Destroy(bobber);

        bobber = null;
        rod = null;
        fishItem = null;

        ShowStatus(message);
    }

    private void SetFishingAnimation(int mode)
    {
        if (animator != null)
            animator.SetInteger(FishingModeHash, mode);
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        if (!string.IsNullOrEmpty(message))
            Debug.Log(message, this);
    }

    private bool TryRollFish(out ItemData_SO result)
    {
        result = null;
        int totalWeight = 0;

        if (fishTable == null || fishTable.Length == 0)
        {
            Debug.LogError("PlayerFishing의 Fish Table에 물고기를 등록해주세요.", this);
            return false;
        }

        for (int i = 0; i < fishTable.Length; i++)
        {
            FishEntry entry = fishTable[i];

            if (entry == null || entry.weight <= 0)
                continue;

            ItemData_SO item = entry.item;

            if (item == null ||
                string.IsNullOrWhiteSpace(item.Id) ||
                inventory.GetItemData(item.Id) != item ||
                item.ItemType == ItemTypeSO.Tool ||
                item.ItemType == ItemTypeSO.None)
            {
                Debug.LogError(
                    $"Fish Table의 Element {i}에 ItemDatabase에 등록된 물고기를 연결해주세요.",
                    this);
                return false;
            }

            totalWeight += entry.weight;
        }

        if (totalWeight == 0)
        {
            Debug.LogError("Fish Table에 가중치가 1 이상인 물고기가 필요해요.", this);
            return false;
        }

        int roll = Random.Range(0, totalWeight);

        foreach (FishEntry entry in fishTable)
        {
            if (entry == null || entry.weight <= 0)
                continue;

            if (roll < entry.weight)
            {
                result = entry.item;
                return true;
            }

            roll -= entry.weight;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin =
            transform.position
            + transform.forward * castDistance
            + Vector3.up * rayHeight;

        bool hitWater = Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            rayDistance,
            waterLayer,
            QueryTriggerInteraction.Collide);

        // 물 감지 성공: 초록 / 실패: 빨강
        Gizmos.color = hitWater ? Color.green : Color.red;

        Vector3 endPoint = hitWater
            ? hit.point
            : origin + Vector3.down * rayDistance;

        Gizmos.DrawLine(origin, endPoint);
        Gizmos.DrawWireSphere(endPoint, 0.1f);
    }
}