using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [Header("<< Dialogue Text >>")]
    [SerializeField] private TMP_Text dialogueText;

    [Header("<< Choice >>")]
    [SerializeField] private YesOrNoChoice yesOrNoChoice;

    private string yesDialogue;
    private string noDialogue;

    private void Awake()
    {
        yesOrNoChoice.OnChoiceConfirmed += OnYesOrNoSelected;
    }

    private void OnDestroy()
    {
        yesOrNoChoice.OnChoiceConfirmed -= OnYesOrNoSelected;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogue();
        }
    }

    public void OpenDialogue(
        string dialogue,
        string yesDialogue,
        string noDialogue)
    {
        dialogueText.text = dialogue;

        this.yesDialogue = yesDialogue;
        this.noDialogue = noDialogue;

        gameObject.SetActive(true);

        yesOrNoChoice.gameObject.SetActive(true);
    }

    private void OnYesOrNoSelected(bool isYes)
    {
        if (isYes)
        {
            dialogueText.text = yesDialogue;

            if (NPCManager.Instance != null)
            {
                NPCManager.Instance.OnDialogueYes();
            }
        }
        else
        {
            dialogueText.text = noDialogue;
        }

        // ¼±ÅÃ ¿Ï·á ¡æ ¼±ÅÃ UI ¼û±è
        yesOrNoChoice.gameObject.SetActive(false);
    }

    public void CloseDialogue()
    {
        yesOrNoChoice.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }
}