using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogue();
        }
    }

    public void OpenDialogue()
    {
        gameObject.SetActive(true);
    }

    public void CloseDialogue()
    {
        gameObject.SetActive(false);
    }
}