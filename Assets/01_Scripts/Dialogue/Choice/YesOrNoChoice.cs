using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class YesOrNoChoice : MonoBehaviour
{
    [Header("<< Buttons >>")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("<< Selection Arrow >>")]
    [SerializeField] private GameObject yesArrow;
    [SerializeField] private GameObject noArrow;

    private int selectedIndex = 0;

    public event Action<bool> OnChoiceConfirmed;

    private void Awake()
    {
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    private void OnDestroy()
    {
        yesButton.onClick.RemoveListener(OnYesClicked);
        noButton.onClick.RemoveListener(OnNoClicked);
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

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedIndex = 0;
            UpdateSelection();
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedIndex = 1;
            UpdateSelection();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    private void UpdateSelection()
    {
        yesArrow.SetActive(selectedIndex == 0);
        noArrow.SetActive(selectedIndex == 1);
    }

    private void OnYesClicked()
    {
        selectedIndex = 0;
        UpdateSelection();

        ConfirmSelection();
    }

    private void OnNoClicked()
    {
        selectedIndex = 1;
        UpdateSelection();

        ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        bool isYes = selectedIndex == 0;

        Debug.Log(isYes ? "예 선택" : "아니오 선택");

        OnChoiceConfirmed?.Invoke(isYes);
    }
}