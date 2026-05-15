using System.Collections;
using UnityEngine;

public class Fridge : Interactable
{
    [Header("<< ≥√¿Â∞Ì πÆ >>")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("<< πÆ º≥¡§ >>")]
    [SerializeField] private float openAngle = 120f;
    [SerializeField] private float openSpeed = 2f;

    private bool isOpen = false;
    private Coroutine doorCoroutine;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;

    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    protected override void Start()
    {
        base.Start();

        if (leftDoor != null)
        {
            leftClosedRot = leftDoor.localRotation;
            leftOpenRot = leftClosedRot * Quaternion.Euler(0f, openAngle, 0f);
        }

        if (rightDoor != null)
        {
            rightClosedRot = rightDoor.localRotation;
            rightOpenRot = rightClosedRot * Quaternion.Euler(0f, -openAngle, 0f);
        }
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

        doorCoroutine = StartCoroutine(RotateDoors(leftOpenRot, rightOpenRot));

        Debug.Log("≥√¿Â∞Ì πÆ¿ª ø±¥œ¥Ÿ.");
    }

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (doorCoroutine != null)
            StopCoroutine(doorCoroutine);

        doorCoroutine = StartCoroutine(RotateDoors(leftClosedRot, rightClosedRot));

        Debug.Log("≥√¿Â∞Ì πÆ¿ª ¥›Ω¿¥œ¥Ÿ.");
    }

    private IEnumerator RotateDoors(Quaternion leftTargetRot, Quaternion rightTargetRot)
    {
        while (true)
        {
            bool leftDone = true;
            bool rightDone = true;

            if (leftDoor != null)
            {
                leftDoor.localRotation = Quaternion.Lerp(
                    leftDoor.localRotation,
                    leftTargetRot,
                    Time.deltaTime * openSpeed
                );

                leftDone = Quaternion.Angle(leftDoor.localRotation, leftTargetRot) < 0.1f;
            }

            if (rightDoor != null)
            {
                rightDoor.localRotation = Quaternion.Lerp(
                    rightDoor.localRotation,
                    rightTargetRot,
                    Time.deltaTime * openSpeed
                );

                rightDone = Quaternion.Angle(rightDoor.localRotation, rightTargetRot) < 0.1f;
            }

            if (leftDone && rightDone)
                break;

            yield return null;
        }

        if (leftDoor != null)
            leftDoor.localRotation = leftTargetRot;

        if (rightDoor != null)
            rightDoor.localRotation = rightTargetRot;
    }
}