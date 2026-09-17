using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    private NPC currentNPC;  // 현재 상호작용 할 수 있는 NPC 

    public NPC CurrentNPC => currentNPC; 

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetCurrentNPC(NPC npc)
    {
        currentNPC = npc;
    }

    public void ClearCurrentNPC(NPC npc)
    {
        if(currentNPC == npc)
        {
            currentNPC = null;
        }
    }

    public void InteractWithCurrentNPC()
    {
        if (currentNPC == null)
            return;

        switch (currentNPC.NPCType)
        {
            case NPCType.FishShop:
                Debug.Log("Fish 상점 NPC와 상호작용");
                break;

            case NPCType.GroceryShop:
                Debug.Log("Grocery 상점 NPC와 상호작용");
                break;

            case NPCType.WeaponShop:
                Debug.Log("Weapon 상점 NPC와 상호작용");
                break;

            case NPCType.Fisherman:
                Debug.Log("Fisherman NPC와 상호작용");

                FishermanController fisherman = currentNPC.GetComponent<FishermanController>();

                if (fisherman != null)
                {
                    fisherman.StartInteraction();
                }

                break;

            case NPCType.Pirate:
                Debug.Log("Pirate NPC와 상호작용");
                break;

            case NPCType.TrainDriver:
                Debug.Log("TrainDriver NPC와 상호작용");
                break;

            case NPCType.Banker1:
                Debug.Log("Banker1 NPC와 상호작용");
                break;

            case NPCType.Banker2:
                Debug.Log("Banker2 NPC와 상호작용");
                break;

            case NPCType.StoreManager:
                Debug.Log("StoreManager와 상호작용");
                break;

            case NPCType.PartTimeWorker:
                Debug.Log("PartTimeWorker NPC와 상호작용");
                break;

            case NPCType.Hunter:
                Debug.Log("Hunter NPC와 상호작용");
                break; 
        }
    }
}
