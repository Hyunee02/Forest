using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [Header("<< Dialogue Text >>")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text npcNameText;

    [Header("<< Choice >>")]
    [SerializeField] private Choice2 choice2;
    [SerializeField] private Choice3 choice3;

    private DialogueChoiceData[] choices;

    private PlayerMove playerMove;

    private void Awake()
    {
        choice2.OnChoiceConfirmed += OnChoiceConfirmed;
        choice3.OnChoiceConfirmed += OnChoiceConfirmed;
    }

    private void OnDestroy()
    {
        choice2.OnChoiceConfirmed -= OnChoiceConfirmed;
        choice3.OnChoiceConfirmed -= OnChoiceConfirmed;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogue();
        }
    }

    public void OpenDialogue(string dialogue, DialogueChoiceData[] choices)
    {
        dialogueText.text = dialogue;

        if (NPCManager.Instance != null)
        {
            NPC npc = NPCManager.Instance.CurrentNPC;

            if (npc != null && npc.DialogueData != null)
            {
                npcNameText.text = npc.DialogueData.npcName;
            }
        }

        this.choices = choices;

        LockPlayerMove();

        gameObject.SetActive(true);

        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);

        if (choices == null || choices.Length == 0)
            return;

        if (choices.Length == 2)
        {
            choice2.SetChoices(choices);
            choice2.gameObject.SetActive(true);
        }
        else if (choices.Length == 3)
        {
            choice3.SetChoices(choices);
            choice3.gameObject.SetActive(true);
        }
    }

    private void OnChoiceConfirmed(int index)
    {
        if (choices == null)
            return;

        if (index < 0 || index >= choices.Length)
            return;

        dialogueText.text = choices[index].dialogue;

        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);

        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.OnDialogueChoice(index);
        }
    }

    public void ShowByeDialogue()
    {
        if (NPCManager.Instance == null)
            return;

        NPC npc = NPCManager.Instance.CurrentNPC;

        if (npc == null || npc.DialogueData == null)
            return;

        dialogueText.text = npc.DialogueData.GetRandomBye();

        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);

        CloseDialogueAfterDelay(1.5f);
    }

    public void CloseDialogueAfterDelay(float delay)
    {
        StartCoroutine(CloseDialogueCoroutine(delay));
    }

    private IEnumerator CloseDialogueCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        CloseDialogue();
    }

    public void CloseDialogue()
    {
        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);

        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.OnDialogueClose();
        }

        UnlockPlayerMove();

        gameObject.SetActive(false);
    }

    #region < Lock Player Move >

    private void LockPlayerMove()
    {
        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
            return;

        playerMove =
            playerObj.GetComponent<PlayerMove>();

        if (playerMove == null)
            return;

        playerMove.SetMoveEnabled(false);
    }

    private void UnlockPlayerMove()
    {
        if (playerMove == null)
        {
            GameObject playerObj =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                playerMove =
                    playerObj.GetComponent<PlayerMove>();
            }
        }

        if (playerMove != null)
        {
            playerMove.SetMoveEnabled(true);
        }
    }

    #endregion
}