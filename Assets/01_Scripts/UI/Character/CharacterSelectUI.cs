using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Serializable]
    private class CharacterSlot
    {
        public Button button;
        public Image portrait;
        public GameObject selectedMark;
    }

    [SerializeField] private CharacterDatabase database;
    [SerializeField] private CharacterPreview preview;
    [SerializeField] private CharacterSlot[] slots;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button confirmButton;

    public CharacterCreateDraft Draft { get; } = new CharacterCreateDraft();
    public bool IsConfirmed { get; private set; }

    private UnityAction[] clicks;
    private int selected = -1;

    private void Start()
    {
        if (confirmButton != null)
            confirmButton.interactable = false;

        if (!ValidateSetup(out string error))
        {
            if (statusText != null)
                statusText.text = error;

            Debug.LogError(error, this);
            return;
        }

        clicks = new UnityAction[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;

            slots[i].portrait.sprite = database.Characters[i].Portrait;
            slots[i].portrait.preserveAspect = true;
            slots[i].selectedMark.SetActive(false);

            clicks[i] = () => Select(index);
            slots[i].button.onClick.AddListener(clicks[i]);
        }

        confirmButton.onClick.AddListener(Confirm);
        statusText.text = "함께할 캐릭터를 골라주세요.";
    }

    private bool ValidateSetup(out string error)
    {
        error = "캐릭터 선택 화면의 연결을 확인해주세요.";

        if (preview == null || statusText == null ||
            confirmButton == null || database == null)
        {
            return false;
        }

        if (!database.ValidateDatabase(out error))
            return false;

        error = "캐릭터 수와 슬롯 수를 맞춰주세요.";

        if (slots == null || slots.Length != database.Characters.Count)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            CharacterSlot slot = slots[i];
            error = $"{i + 1}번째 슬롯과 캐릭터 초상화를 연결해주세요.";

            if (slot == null || slot.button == null ||
                slot.portrait == null || slot.selectedMark == null ||
                database.Characters[i].Portrait == null)
            {
                return false;
            }
        }

        error = null;
        return true;
    }

    private void Select(int index)
    {
        if (selected == index)
            return;

        selected = index;
        IsConfirmed = false;
        Draft.characterId = null;

        for (int i = 0; i < slots.Length; i++)
            slots[i].selectedMark.SetActive(i == index);

        CharacterData data = database.Characters[index];

        preview.Show(data);
        confirmButton.interactable = true;
        statusText.text = $"{data.CharacterName} 캐릭터로 결정할까요?";
    }

    private void Confirm()
    {
        if (selected < 0)
            return;

        Draft.characterId = database.Characters[selected].ID;
        IsConfirmed = true;
        confirmButton.interactable = false;
        statusText.text = "캐릭터를 선택했어요.";
    }

    private void OnDestroy()
    {
        if (clicks == null)
            return;

        for (int i = 0; i < clicks.Length; i++)
        {
            if (slots[i].button != null)
                slots[i].button.onClick.RemoveListener(clicks[i]);
        }

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(Confirm);
    }
}