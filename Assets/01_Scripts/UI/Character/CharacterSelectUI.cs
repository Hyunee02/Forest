using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Serializable]
    private class CharacterOption
    {
        public CharacterPreview preview;
        public Button selectButton;
        public GameObject selectedMark;
    }

    [Header("3D 무대")]
    [SerializeField] private GameObject residentStage;
    [SerializeField] private GameObject selectionStage;

    [Header("주민 대화창")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [TextArea(2, 4)]
    [SerializeField]
    private string[] welcomeMessages =
    {
        "어서 와! 배 타고 오느라 고생 많았지?",
        "나는 오늘 항구에서 새 주민을 맞이하고 있어.",
        "마을에 들어가기 전에 주민 명부부터 같이 적어볼까?"
    };

    [SerializeField, Min(0.1f)] private float messageSeconds = 2.5f;

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
    [SerializeField] private TMP_Text selectionText;

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

        if (!ValidateCharacters(out string error))
        {
            dialogueText.text = error;
            Debug.LogError(error, this);
            return;
        }

        leftOption.selectedMark.SetActive(false);
        rightOption.selectedMark.SetActive(false);
        confirmButton.interactable = false;

        nameNextButton.onClick.AddListener(ConfirmName);
        birthdayNextButton.onClick.AddListener(ConfirmBirthday);
        leftOption.selectButton.onClick.AddListener(SelectLeft);
        rightOption.selectButton.onClick.AddListener(SelectRight);
        confirmButton.onClick.AddListener(EnterGame);

        StartCoroutine(PlayWelcome());
    }

    private bool ValidateCharacters(out string error)
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
            error = "캐릭터 선택 화면에 사용할 캐릭터를 두 명 등록해주세요.";
            return false;
        }

        error = null;
        return true;
    }

    private IEnumerator PlayWelcome()
    {
        foreach (string message in welcomeMessages)
        {
            if (string.IsNullOrWhiteSpace(message))
                continue;

            dialogueText.text = message;
            yield return new WaitForSecondsRealtime(messageSeconds);
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
        if (!birthdayPanel.TryGetBirthday(out DateTime birthday, out string error))
        {
            dialogueText.text = error;
            return;
        }

        draft.birthYear = birthday.Year;
        draft.birthMonth = birthday.Month;
        draft.birthDay = birthday.Day;

        birthdayPanel.gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
        residentStage.SetActive(false);

        selectionStage.SetActive(true);
        selectionPanel.SetActive(true);

        leftOption.preview.Show(database.Characters[0]);
        rightOption.preview.Show(database.Characters[1]);

        selectionText.text = "함께할 캐릭터를 골라주세요.";
    }

    private void SelectLeft()
    {
        SelectCharacter(0);
    }

    private void SelectRight()
    {
        SelectCharacter(1);
    }

    private void SelectCharacter(int index)
    {
        if (loading)
            return;

        selectedIndex = index;

        leftOption.selectedMark.SetActive(index == 0);
        rightOption.selectedMark.SetActive(index == 1);
        confirmButton.interactable = true;

        CharacterData selected = database.Characters[index];
        selectionText.text = $"{selected.CharacterName} 캐릭터로 시작할까요?";
    }

    private void EnterGame()
    {
        if (loading || selectedIndex < 0)
            return;

        if (!Application.CanStreamedLevelBeLoaded(playerSceneName))
        {
            selectionText.text = $"씬 목록에서 '{playerSceneName}'을 확인해주세요.";
            return;
        }

        loading = true;
        confirmButton.interactable = false;

        draft.characterId = database.Characters[selectedIndex].ID;
        GameSession.SetProfile(draft);

        SceneManager.LoadSceneAsync(playerSceneName, LoadSceneMode.Single);
    }
}