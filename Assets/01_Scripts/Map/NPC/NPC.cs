using UnityEngine;

public enum NPCType
{
    FishShop,
    GroceryShop,
    WeaponShop,
    Fisherman,
    Pirate,
    TrainDriver,
    Banker1,
    Banker2,
    StoreManager,
    PartTimeWorker,
}

public class NPC : Interactable
{
    [Header("<< NPC 타입 >>")]
    [SerializeField] private NPCType npcType;

    public NPCType NPCType => npcType;

    protected override void Interact()
    {
        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.InteractWithCurrentNPC();
        }
    }

    protected override void OnEnterInteractionRange()
    {
        Debug.Log($"{npcType} NPC 상호작용 범위에 들어옴");

        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.SetCurrentNPC(this);
        }
    }

    protected override void OnExitInteractionRange()
    {
        Debug.Log($"{npcType} NPC 상호작용 범위에서 나감");

        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.ClearCurrentNPC(this);
        }
    }
}