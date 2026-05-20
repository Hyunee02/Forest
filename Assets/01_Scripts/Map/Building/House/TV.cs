using UnityEngine;

public class TV : Interactable
{
    [Header("<< TV 상태 오브젝트 >>")]
    [SerializeField] private GameObject TVOnObject;
    [SerializeField] private GameObject TVOffObject;

    [Header("<< 초기 상태 >>")]
    [SerializeField] private bool isTVOn = false;

    protected override void Start()
    {
        base.Start();

        ApplyTVState();
    }

    protected override void Interact()
    {
        ToggleTV();
    }

    private void ToggleTV()
    {
        isTVOn = !isTVOn;

        ApplyTVState();
    }

    private void ApplyTVState()
    {
        if (TVOnObject != null)
            TVOnObject.SetActive(isTVOn);

        if (TVOffObject != null)
            TVOffObject.SetActive(!isTVOn);
    }
}
