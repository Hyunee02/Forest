using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Choice2 : MonoBehaviour
{
    [Header("<< Buttons >>")]
    [SerializeField] private Button choice1Button;
    [SerializeField] private Button choice2Button;

    [Header("<< Selection Arrow >>")]
    [SerializeField] private GameObject choice1Arrow;
    [SerializeField] private GameObject choice2Arrow;

    [Header("<< Choice Text >>")]
    [SerializeField] private TMPro.TMP_Text choice1Text;
    [SerializeField] private TMPro.TMP_Text choice2Text;

    private int selectedIndex = 0;

    private DialogueChoiceData[] choices;

    public event Action<int> OnChoiceConfirmed;

    private void Awake()
    {
        choice1Button.onClick.AddListener(OnChoice1Clicked);
        choice2Button.onClick.AddListener(OnChoice2Clicked);
    }

    private void OnDestroy()
    {
        choice1Button.onClick.RemoveListener(OnChoice1Clicked);
        choice2Button.onClick.RemoveListener(OnChoice2Clicked);
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
            selectedIndex = 0;
            UpdateSelection();
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame)
        {
            selectedIndex = 1;
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

        if (choices == null || choices.Length < 2)
            return;

        choice1Text.text = choices[0].choiceText;
        choice2Text.text = choices[1].choiceText;
    }

    private void UpdateSelection()
    {
        choice1Arrow.SetActive(selectedIndex == 0);
        choice2Arrow.SetActive(selectedIndex == 1);
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

    private void ConfirmSelection()
    {
        if (choices == null || choices.Length < 2)
            return;

        OnChoiceConfirmed?.Invoke(selectedIndex);
    }
}