using TMPro;
using UnityEngine;

[RequireComponent(typeof(PlayerEquip))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerFishing : MonoBehaviour
{
    [Header("물 판정")]
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private float castDistance = 3f;
    [SerializeField] private float rayHeight = 3f;
    [SerializeField] private float rayDistance = 6f;

    [Header("찌")]
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField] private float surfaceOffset = 0.05f;
    [SerializeField] private float biteDepth = 0.2f;

    [Header("획득할 물고기")]
    [SerializeField] private ItemData_SO fishItem;

    [Header("입질")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private float reactionTime = 1.5f;

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
        if (!IsFishing)
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
        if (!isActiveAndEnabled || Time.timeScale <= 0f)
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

        if (!hasBite)
        {
            FinishFishing("찌를 회수했어요.");
            return;
        }

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

        if (bobberPrefab == null || fishItem == null)
        {
            Debug.LogError(
                "PlayerFishing의 Bobber Prefab과 Fish Item을 연결해주세요.",
                this);
            return;
        }

        ItemData_SO data = inventory.GetItemData(fishItem.Id);

        if (data == null
            || data.ItemType == ItemTypeSO.Tool
            || data.ItemType == ItemTypeSO.None)
        {
            Debug.LogError(
                "물고기를 ItemDatabase의 Items에 등록해주세요.",
                this);
            return;
        }

        if (!inventory.CanAddItem(fishItem.Id, 1))
        {
            ShowStatus("인벤토리가 가득 찼어요.");
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

        // 파손 이벤트가 발생하기 전에 낚시 상태 정리
        FinishFishing($"{fishName} 획득!");

        SetFishingAnimation(2);

        equip.ReduceEquippedDurability(reduce);
    }

    private void CheckEquipment()
    {
        if (IsFishing && equip.CurTool != rod)
            AbortFishing();
    }

    public void CancelFishing()
    {
        if (IsFishing)
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
        hasBite = false;

        SetFishingAnimation(0);

        if (bobber != null)
            Destroy(bobber);

        bobber = null;
        rod = null;

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