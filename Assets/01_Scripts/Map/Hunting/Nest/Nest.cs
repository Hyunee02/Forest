using UnityEngine;

public class Nest : Interactable
{
    protected override void Interact()
    {
        GetEgg();
    }

    private void GetEgg()
    {
        Debug.Log("알을 얻었습니다.");
    }
}
