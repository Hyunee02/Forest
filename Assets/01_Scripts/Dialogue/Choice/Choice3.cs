using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Choice3 : MonoBehaviour
{
    [Header("<< Buttons >>")]
    [SerializeField] private Button choice1Button;
    [SerializeField] private Button choice2Button;
    [SerializeField] private Button choice3Button;

    [Header("<< Selection Arrow >>")]
    [SerializeField] private GameObject choice1Arrow;
    [SerializeField] private GameObject choice2Arrow;
    [SerializeField] private GameObject choice3Arrow;

    [Header("<< Choice Text >>")]
    [SerializeField] private TMP_Text choice1Text;
    [SerializeField] private TMP_Text choice2Text;
    [SerializeField] private TMP_Text choice3Text;

    private int selectedIndex = 0;

    private DialogueChoiceData[] choices;

    public event Action<int> OnChoiceConfirmed;

    private void Awake()
    {
        choice1Button.onClick.AddListener(OnChoice1Clicked);
        choice2Button.onClick.AddListener(OnChoice2Clicked);
        choice3Button.onClick.AddListener(OnChoice3Clicked);
    }

    private void OnDestroy()
    {
        choice1Button.onClick.RemoveListener(OnChoice1Clicked);
        choice2Button.onClick.RemoveListener(OnChoice2Clicked);
        choice3Button.onClick.RemoveListener(OnChoice3Clicked);
    }

    private void OnEnable()
    {
        selectedIndex = 0;
        UpdateSelection();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame)
        {
            selectedIndex--;

            if (selectedIndex < 0)
                selectedIndex = 2;

            UpdateSelection();
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame)
        {
            selectedIndex++;

            if (selectedIndex > 2)
                selectedIndex = 0;

            UpdateSelection();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    public void SetChoices(DialogueChoiceData[] choices)
    {
        this.choices = choices;

        if (choices == null || choices.Length < 3)
            return;

        choice1Text.text = choices[0].choiceText;
        choice2Text.text = choices[1].choiceText;
        choice3Text.text = choices[2].choiceText;
    }

    private void UpdateSelection()
    {
        choice1Arrow.SetActive(selectedIndex == 0);
        choice2Arrow.SetActive(selectedIndex == 1);
        choice3Arrow.SetActive(selectedIndex == 2);
    }

    private void OnChoice1Clicked()
    {
        selectedIndex = 0;
        UpdateSelection();
        ConfirmSelection();
    }

    private void OnChoice2Clicked()
    {
        selectedIndex = 1;
        UpdateSelection();
        ConfirmSelection();
    }

    private void OnChoice3Clicked()
    {
        selectedIndex = 2;
        UpdateSelection();
        ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        if (choices == null || choices.Length < 3)
            return;

        OnChoiceConfirmed?.Invoke(selectedIndex);
    }
}