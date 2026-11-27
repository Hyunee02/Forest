using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Serializable]
    private class CharacterOption
    {
        public CharacterPreview preview;
        public CharacterSelectArea area;
        public GameObject selectedMark;
    }

    [Header("3D 무대")]
    [SerializeField] private GameObject residentStage;
    [SerializeField] private GameObject selectionStage;

    [Header("주민 대화창")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("시작 대사")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] welcomeMessages =
    {
        "어서 와! 배 타고 오느라 고생 많았지?",
        "나는 오늘 항구에서 새 주민을 맞이하고 있어.",
        "마을에 들어가기 전에 주민 명부부터 같이 적어볼까?"
    };

    [Header("캐릭터 선택 직전 대사")]
    [TextArea(2, 4)]
    [SerializeField]
    private string appearanceQuestion =
        "그럼 마지막으로, 네 모습은 어떤 모습이야?";

    [Header("이름 입력")]
    [SerializeField] private GameObject namePanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button nameNextButton;

    [Header("생년월일 입력")]
    [SerializeField] private BirthdayInputUI birthdayPanel;
    [SerializeField] private Button birthdayNextButton;

    [Header("캐릭터 선택")]
    [SerializeField] private CharacterDatabase database;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private CharacterOption leftOption;
    [SerializeField] private CharacterOption rightOption;
    [SerializeField] private Button confirmButton;

    [Header("씬")]
    [SerializeField] private string playerSceneName = "Player";

    private readonly CharacterCreateDraft draft = new CharacterCreateDraft();

    private int selectedIndex = -1;
    private bool loading;

    private void Start()
    {
        residentStage.SetActive(true);
        selectionStage.SetActive(false);

        dialoguePanel.SetActive(true);
        namePanel.SetActive(false);
        birthdayPanel.gameObject.SetActive(false);
        selectionPanel.SetActive(false);

        if (!ValidateSelection(out string error))
        {
            dialogueText.text = error;
            Debug.LogError(error, this);
            return;
        }

        leftOption.area.Init(this, 0);
        rightOption.area.Init(this, 1);

        nameNextButton.onClick.AddListener(ConfirmName);
        birthdayNextButton.onClick.AddListener(ConfirmBirthday);
        confirmButton.onClick.AddListener(ConfirmCharacter);

        StartCoroutine(PlayWelcome());
    }

    private void OnDestroy()
    {
        if (nameNextButton != null)
            nameNextButton.onClick.RemoveListener(ConfirmName);

        if (birthdayNextButton != null)
            birthdayNextButton.onClick.RemoveListener(ConfirmBirthday);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(ConfirmCharacter);
    }

    private bool ValidateSelection(out string error)
    {
        if (database == null)
        {
            error = "Character Database를 연결해주세요.";
            return false;
        }

        if (!database.ValidateDatabase(out error))
            return false;

        if (database.Characters.Count != 2)
        {
            error = "Character Database에 캐릭터 두 명을 등록해주세요.";
            return false;
        }

        if (leftOption == null || rightOption == null ||
            leftOption.preview == null || rightOption.preview == null ||
            leftOption.area == null || rightOption.area == null ||
            leftOption.selectedMark == null || rightOption.selectedMark == null)
        {
            error = "Left Option과 Right Option에 Preview, Area, Selected Mark를 연결해주세요.";
            return false;
        }

        if (leftOption.preview == rightOption.preview ||
            leftOption.area == rightOption.area ||
            leftOption.selectedMark == rightOption.selectedMark)
        {
            error = "왼쪽과 오른쪽에 각각 다른 Preview, Area, Selected Mark를 연결해주세요.";
            return false;
        }

        error = null;
        return true;
    }

    // 대사를 표시한 다음 프레임부터 새 클릭을 기다린다.
    // 생일 다음 버튼이나 확정 버튼을 누른 입력으로 대사가 바로 넘어가는 것을 방지한다.
    private IEnumerator ShowDialogueAndWait(string message)
    {
        dialogueText.text = message;

        do
        {
            yield return null;
        }
        while (!WasAdvancePressed());
    }

    private bool WasAdvancePressed()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;

        Touchscreen touchscreen = Touchscreen.current;
        return touchscreen != null &&
               touchscreen.primaryTouch.press.wasPressedThisFrame;
    }

    private IEnumerator PlayWelcome()
    {
        foreach (string message in welcomeMessages)
        {
            if (string.IsNullOrWhiteSpace(message))
                continue;

            yield return ShowDialogueAndWait(message);
        }

        dialogueText.text = "마을에서 뭐라고 불러주면 될까?";
        namePanel.SetActive(true);

        nameInput.Select();
        nameInput.ActivateInputField();
    }

    private void ConfirmName()
    {
        string value = nameInput.text.Trim();

        if (value.Length == 0)
        {
            dialogueText.text = "이름을 먼저 알려줘!";
            nameInput.ActivateInputField();
            return;
        }

        if (new StringInfo(value).LengthInTextElements > 10)
        {
            dialogueText.text = "이름은 열 글자까지 적을 수 있어.";
            nameInput.ActivateInputField();
            return;
        }

        draft.playerName = value;

        namePanel.SetActive(false);
        dialogueText.text = $"{draft.playerName}, 만나서 반가워! 생일은 언제야?";
        birthdayPanel.gameObject.SetActive(true);
    }

    private void ConfirmBirthday()
    {
        if (!birthdayPanel.gameObject.activeInHierarchy)
            return;

        if (!birthdayPanel.TryGetBirthday(out DateTime birthday, out string error))
        {
            dialogueText.text = error;
            return;
        }

        draft.birthYear = birthday.Year;
        draft.birthMonth = birthday.Month;
        draft.birthDay = birthday.Day;

        birthdayPanel.gameObject.SetActive(false);
        StartCoroutine(AskAppearanceAndOpenSelection());
    }

    private IEnumerator AskAppearanceAndOpenSelection()
    {
        yield return ShowDialogueAndWait(appearanceQuestion);

        dialoguePanel.SetActive(false);
        residentStage.SetActive(false);
        selectionStage.SetActive(true);

        leftOption.preview.Show(database.Characters[0]);
        rightOption.preview.Show(database.Characters[1]);

        selectedIndex = -1;
        leftOption.selectedMark.SetActive(false);
        rightOption.selectedMark.SetActive(false);
        confirmButton.interactable = false;

        selectionPanel.SetActive(true);
    }

    // CharacterSelectArea에서 왼쪽은 0, 오른쪽은 1을 전달한다.
    public void SelectCharacter(int index)
    {
        if (loading || !selectionPanel.activeInHierarchy)
            return;

        if (index != 0 && index != 1)
            return;

        selectedIndex = index;

        leftOption.selectedMark.SetActive(index == 0);
        rightOption.selectedMark.SetActive(index == 1);
        confirmButton.interactable = true;
    }

    private void ConfirmCharacter()
    {
        if (loading || selectedIndex < 0)
            return;

        if (!Application.CanStreamedLevelBeLoaded(playerSceneName))
        {
            string error = "";
            dialoguePanel.SetActive(true);
            dialogueText.text = error;
            Debug.LogError(error, this);
            return;
        }

        loading = true;
        confirmButton.interactable = false;

        draft.characterId = database.Characters[selectedIndex].ID;
        GameSession.SetProfile(draft);

        StartCoroutine(PlayDeparture());
    }

    private IEnumerator PlayDeparture()
    {
        selectionPanel.SetActive(false);
        selectionStage.SetActive(false);

        residentStage.SetActive(true);
        dialoguePanel.SetActive(true);

        yield return ShowDialogueAndWait(
            $"{draft.playerName}, 주민증이 완성됐어! 이제 마을로 가볼까?"
        );

        yield return SceneManager.LoadSceneAsync(
            playerSceneName,
            LoadSceneMode.Single
        );
    }
}