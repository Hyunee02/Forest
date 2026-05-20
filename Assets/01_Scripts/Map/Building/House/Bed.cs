using UnityEngine;

public class Bed : Interactable
{
    protected override void Interact()
    {
        UseBed();
    }

    private void UseBed()
    {
        Debug.Log("Bed를 사용합니다.");
    }
}
