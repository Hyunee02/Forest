using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("<< Dialogue >>")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("<< Shop >>")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private float shopOpenDelay = 1f;

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

    #region < Current NPC >

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

    #endregion


    #region < Interact >

    public void InteractWithCurrentNPC()
    {
        if (currentNPC == null)
            return;

        LookAtPlayer(currentNPC.transform);

        if (currentNPC.DialogueData != null && dialogueUI != null)
        {
            string dialogue =  currentNPC.DialogueData.GetRandomGreeting();

            dialogueUI.OpenDialogue(dialogue, currentNPC.DialogueData.choices);
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

    #endregion


    #region < LookAtPlayer >

    private void LookAtPlayer(Transform npcTransform)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
            return;

        Vector3 direction = playerObj.transform.position - npcTransform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        npcTransform.rotation = Quaternion.LookRotation(direction);
    }

    #endregion

    #region < Dialogue Choice >

    public void OnDialogueChoice(int index)
    {
        if (currentNPC == null)
            return;

        switch (currentNPC.NPCType)
        {
            //==================================================
            // Pirate
            // 1번 : HuntingGround 이동
            // 2번 : 취소
            //==================================================
            case NPCType.Pirate:

                if (index == 0)
                {
                    StartCoroutine(MoveToScene(huntingGroundSceneName));
                }
                else if (index == 1)
                {
                    currentNPC.RestoreOriginalRotation();

                    if (dialogueUI != null)
                    {
                        dialogueUI.CloseDialogueAfterDelay(1.5f);
                    }
                }

                break;


            //==================================================
            // TrainDriver
            // 1번 : Dungeon 이동
            // 2번 : 취소
            //==================================================
            case NPCType.TrainDriver:

                if (index == 0)
                {
                    StartCoroutine(MoveToScene(dungeonSceneName));
                }
                else if (index == 1)
                {
                    currentNPC.RestoreOriginalRotation();

                    if (dialogueUI != null)
                    {
                        dialogueUI.CloseDialogueAfterDelay(1.5f);
                    }
                }

                break;


            //==================================================
            // Fisherman
            // 1번 : 낚시하는 법
            // 2번 : 미끼 구매
            // 3번 : 취소
            //==================================================
            case NPCType.Fisherman:

                if (index == 0)
                {
                    Debug.Log("낚시하는 법 선택");
                }
                else if (index == 1)
                {
                    Debug.Log("미끼 구매하기 선택");
                }
                else if (index == 2)
                {
                    currentNPC.RestoreOriginalRotation();

                    if (dialogueUI != null)
                    {
                        dialogueUI.CloseDialogueAfterDelay(1.5f);
                    }
                }

                break;

            //==================================================
            // Shop NPC
            // 1번 : 구매하기
            // 2번 : 판매하기
            // 3번 : 취소하기
            //==================================================
            case NPCType.FishShop:
            case NPCType.GroceryShop:
            case NPCType.WeaponShop:

                if (index == 0)
                {
                    StartCoroutine(OpenShopAfterDelay());
                }
                else if (index == 1)
                {
                    Debug.Log("판매하기");
                }
                else if (index == 2)
                {
                    if (dialogueUI != null)
                    {
                        dialogueUI.CloseDialogueAfterDelay(1.5f);
                    }
                }

                break;
        }
    }

    private IEnumerator OpenShopAfterDelay()
    {
        yield return new WaitForSeconds(shopOpenDelay);

        if (dialogueUI != null)
        {
            dialogueUI.gameObject.SetActive(false);
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
    }

    public void OnDialogueClose()
    {
        if (currentNPC == null)
            return;

        currentNPC.RestoreOriginalRotation();
    }

    private IEnumerator MoveToScene(string sceneName)
    {
        yield return new WaitForSeconds(sceneMoveDelay);

        SceneManager.LoadScene(sceneName);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (dialogueUI != null)
        {
            dialogueUI.gameObject.SetActive(true);
            dialogueUI.ShowByeDialogue();
        }
    }

    #endregion



}