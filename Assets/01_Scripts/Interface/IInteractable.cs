using UnityEngine;

public interface IInteractable
{
    string InteractionText { get; }

    bool CanInteract(GameObject player);
    
    void Interact(GameObject player);
}
