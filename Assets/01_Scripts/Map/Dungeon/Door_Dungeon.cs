using System.Collections;
using UnityEngine;

public class Door_Dungeon : Interactable
{
    [Header("<< 문 오브젝트 >>")]
    [SerializeField] private Transform door;

    [Header("<< 문 회전 설정 >>")]
    [SerializeField] private float closedYAngle = 90f;
    [SerializeField] private float openYAngle = -35f;
    [SerializeField] private float openSpeed = 1.3f;

    [Header("<< 던전 시작 설정 >>")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private bool startDungeonOnOpen = false;

    private bool hasStartedDungeon = false;

    private bool isOpen = false;
    private Coroutine doorCoroutine;

    private Quaternion closedRot;
    private Quaternion openRot;

    protected override void Start()
    {
        base.Start();

        if (door == null)
            door = transform;

        closedRot = Quaternion.Euler(door.localEulerAngles.x, closedYAngle, door.localEulerAngles.z);
        openRot = Quaternion.Euler(door.localEulerAngles.x, openYAngle, door.localEulerAngles.z);

        door.localRotation = closedRot;
    }

    protected override void Interact()
    {
        OpenDoor();
    }

    protected override void OnExitInteractionRange()
    {
        base.OnExitInteractionRange();

        if (isOpen)
            CloseDoor();
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (doorCoroutine != null)
            StopCoroutine(doorCoroutine);

        doorCoroutine = StartCoroutine(RotateDoor(openRot));

        Debug.Log("던전 문을 엽니다.");

        if (startDungeonOnOpen && !hasStartedDungeon)
        {
            hasStartedDungeon = true;

            if (dungeonManager != null)
                dungeonManager.StartDungeonRun();
        }
    }

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (doorCoroutine != null)
            StopCoroutine(doorCoroutine);

        doorCoroutine = StartCoroutine(RotateDoor(closedRot));

        Debug.Log("던전 문을 닫습니다.");
    }

    private IEnumerator RotateDoor(Quaternion targetRot)
    {
        while (Quaternion.Angle(door.localRotation, targetRot) > 0.1f)
        {
            door.localRotation = Quaternion.Lerp(
                door.localRotation,
                targetRot,
                Time.deltaTime * openSpeed
            );

            yield return null;
        }

        door.localRotation = targetRot;
    }
}