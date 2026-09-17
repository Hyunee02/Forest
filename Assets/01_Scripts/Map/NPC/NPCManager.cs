using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    private NPC currentNPC;

    public NPC CurrentNPC => currentNPC;

    private void Awake()
    {
        if (Instance != null && Instance != this)
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
        if (currentNPC == npc)
        {
            currentNPC = null;
        }
    }

    public void InteractWithCurrentNPC()
    {
        if (currentNPC == null)
            return;

        LookAtPlayer(currentNPC.transform);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.OpenDialogue();
        }

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

                FishermanController fisherman =
                    currentNPC.GetComponent<FishermanController>();

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

    private void LookAtPlayer(Transform npcTransform)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Vector3 direction = player.transform.position - npcTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        npcTransform.rotation = Quaternion.LookRotation(direction);
    }
}