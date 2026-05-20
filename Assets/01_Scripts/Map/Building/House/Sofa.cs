using UnityEngine;

public class Sofa : Interactable
{
    protected override void Interact()
    {
        UseSofa();
    }

    private void UseSofa()
    {
        Debug.Log("Sofa 사용합니다.");
    }
}
