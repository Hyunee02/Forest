using UnityEngine;

public class Chair : Interactable
{
    [Header("<< 초기 상태 >>")]
    [SerializeField] private bool isSitOn = false;

    protected override void Interact()
    {
        if (isSitOn)
        {
            isSitOn = false;
            Debug.Log("일어납니다.");
        }
        else
        {
            isSitOn = true;
            Debug.Log("의자에 앉았습니다.");
        }
    }

    public void StandUpByMove()
    {
        if (!isSitOn)
            return;

        isSitOn = false;
        Debug.Log("일어납니다.");
    }
}