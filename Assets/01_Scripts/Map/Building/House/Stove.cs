using UnityEngine;

public class Stove : Interactable
{
    protected override void Interact()
    {
        UseStove();
    }

    private void UseStove()
    {
        Debug.Log("Stove을 사용합니다.");
    }
}
