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
    Hunter,
}

public class NPC : Interactable
{
    [Header("<< NPC 타입 >>")]
    [SerializeField] private NPCType npcType;

    [Header("<< Dialogue >>")]
    [SerializeField] private DialogueData dialogueData;
    public DialogueData DialogueData => dialogueData;

    private Quaternion originalRotation;

    public NPCType NPCType => npcType;

    protected override void Start()
    {
        base.Start();

        originalRotation = transform.rotation;
    }

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

        transform.rotation = originalRotation;

        if (npcType == NPCType.Fisherman)
        {
            FishermanController fisherman =
                GetComponent<FishermanController>();

            if (fisherman != null)
            {
                fisherman.EndInteraction();
            }
        }

        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.ClearCurrentNPC(this);
        }
    }
}