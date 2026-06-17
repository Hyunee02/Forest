using System.Collections;
using UnityEngine;

public class AutoDoor_Dungeon : MonoBehaviour
{
    [Header("<< 문 오브젝트 >>")]
    [SerializeField] private Transform door;

    [Header("<< 회전 설정 >>")]
    [SerializeField] private float closedYAngle = 180f;
    [SerializeField] private float openYAngle = 60f;
    [SerializeField] private float openSpeed = 2f;

    private Coroutine doorCoroutine;

    private Quaternion closedRot;
    private Quaternion openRot;

    private void Awake()
    {
        if (door == null)
            door = transform;

        closedRot = Quaternion.Euler(door.localEulerAngles.x, closedYAngle, door.localEulerAngles.z);
        openRot = Quaternion.Euler(door.localEulerAngles.x, openYAngle, door.localEulerAngles.z);

        door.localRotation = closedRot;
    }

    public void OpenDoor()
    {
        if (doorCoroutine != null)
            StopCoroutine(doorCoroutine);

        doorCoroutine = StartCoroutine(RotateDoor(openRot));

        Debug.Log($"{gameObject.name} 자동 오픈");
    }

    public void CloseInstant()
    {
        if (doorCoroutine != null)
            StopCoroutine(doorCoroutine);

        door.localRotation = closedRot;
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