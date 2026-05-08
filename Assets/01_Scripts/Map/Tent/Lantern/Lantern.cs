using UnityEngine;

public class Lantern : Interactable
{
    [Header("<< 랜턴 상태 오브젝트 >>")]
    [SerializeField] private GameObject lanternOnObject;
    [SerializeField] private GameObject lanternOffObject;

    [Header("<< 초기 상태 >>")]
    [SerializeField] private bool isLanternOn = false;

    protected override void Start()
    {
        base.Start();

        ApplyLanternState();
    }

    protected override void Interact()
    {
        ToggleLantern();
    }

    private void ToggleLantern()
    {
        isLanternOn = !isLanternOn;

        ApplyLanternState();
    }

    private void ApplyLanternState()
    {
        if (lanternOnObject != null)
            lanternOnObject.SetActive(isLanternOn);

        if (lanternOffObject != null)
            lanternOffObject.SetActive(!isLanternOn);
    }
}