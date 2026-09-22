using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("<< Dialogue >>")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("<< 이동할 씬 이름 >>")]
    [SerializeField] private string huntingGroundSceneName = "HuntingGround";
    [SerializeField] private string dungeonSceneName = "Dungeon";

    [Header("<< 이동 딜레이 >>")]
    [SerializeField] private float sceneMoveDelay = 2f;

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

        // NPC가  바라봄
        LookAtPlayer(currentNPC.transform);

        if (currentNPC.DialogueData != null &&
         dialogueUI != null)
        {
            string dialogue =
                currentNPC.DialogueData.GetRandomGreeting();

            dialogueUI.OpenDialogue(
                dialogue,
                currentNPC.DialogueData.yesDialogue,
                currentNPC.DialogueData.noDialogue
            );
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
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
            return;

        Vector3 direction =
            playerObj.transform.position - npcTransform.position;

        // 위아래로 고개를 돌리지 않도록
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        npcTransform.rotation =
            Quaternion.LookRotation(direction);
    }

    public void OnDialogueYes()
    {
        if (currentNPC == null)
            return;

        switch (currentNPC.NPCType)
        {
            case NPCType.Pirate:
                StartCoroutine(MoveToScene(huntingGroundSceneName));
                break;

            case NPCType.TrainDriver:
                StartCoroutine(MoveToScene(dungeonSceneName));
                break;
        }
    }   

    private IEnumerator MoveToScene(string sceneName)
    {
        yield return new WaitForSeconds(sceneMoveDelay);

        SceneManager.LoadScene(sceneName);
    }
}