using UnityEngine;
using UnityEngine.InputSystem;

public class Storage : Interactable
{    protected override void Interact()
    {
        OpenStorage();
    }

    private void OpenStorage()
    {
        Debug.Log("보관함을 열었습니다.");
    }

}
