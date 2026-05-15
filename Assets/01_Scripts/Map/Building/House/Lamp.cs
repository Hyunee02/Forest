using UnityEngine;

public class Lamp : Interactable
{
    [Header("<< 램프 상태 오브젝트 >>")]
    [SerializeField] private GameObject lampOnObject;
    [SerializeField] private GameObject lampOffObject;

    [Header("<< 초기 상태 >>")]
    [SerializeField] private bool isLampOn = false;

    protected override void Start()
    {
        base.Start();

        ApplyLampState();
    }

    protected override void Interact()
    {
        ToggleLamp();
    }

    private void ToggleLamp()
    {
        isLampOn = !isLampOn;

        ApplyLampState();
    }

    private void ApplyLampState()
    {
        if (lampOnObject != null)
            lampOnObject.SetActive(isLampOn);

        if (lampOffObject != null)
            lampOffObject.SetActive(!isLampOn);
    }
}
