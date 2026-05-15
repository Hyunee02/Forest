using UnityEngine;

public class ATM : Interactable
{
    protected override void Interact()
    {
        UseATM();
    }

    private void UseATM()
    {
        Debug.Log("ATM을 사용합니다.");
    }
}
