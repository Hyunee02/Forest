using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BirthdayInputUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private TMP_Dropdown monthDropdown;
    [SerializeField] private TMP_Dropdown dayDropdown;

    private int currentYear;

    private void Awake()
    {
        currentYear = DateTime.Today.Year;

        List<string> years = new List<string> { "연도" };

        for (int year = currentYear; year >= 1900; year--)
            years.Add($"{year}년");

        List<string> months = new List<string> { "월" };

        for (int month = 1; month <= 12; month++)
            months.Add($"{month}월");

        SetOptions(yearDropdown, years, 0);
        SetOptions(monthDropdown, months, 0);
        RefreshDays(0);

        yearDropdown.onValueChanged.AddListener(RefreshDays);
        monthDropdown.onValueChanged.AddListener(RefreshDays);
    }

    private void OnDestroy()
    {
        if (yearDropdown != null)
            yearDropdown.onValueChanged.RemoveListener(RefreshDays);

        if (monthDropdown != null)
            monthDropdown.onValueChanged.RemoveListener(RefreshDays);
    }

    private int GetSelectedYear()
    {
        // 0번은 "연도", 1번이 올해, 2번이 작년이에요.
        return currentYear - yearDropdown.value + 1;
    }

    private void RefreshDays(int unused)
    {
        int previousDay = dayDropdown.value;
        int month = monthDropdown.value;
        bool hasYearAndMonth = yearDropdown.value > 0 && month > 0;

        List<string> days = new List<string> { "일" };

        if (hasYearAndMonth)
        {
            int lastDay = DateTime.DaysInMonth(GetSelectedYear(), month);

            for (int day = 1; day <= lastDay; day++)
                days.Add($"{day}일");
        }

        // 1월 31일 → 2월처럼 기존 날짜가 사라지면 다시 선택하게 해요.
        int selectedDay = previousDay < days.Count ? previousDay : 0;

        SetOptions(dayDropdown, days, selectedDay);
        dayDropdown.interactable = hasYearAndMonth;
    }

    public bool TryGetBirthday(out DateTime birthday, out string error)
    {
        birthday = default;

        if (yearDropdown.value == 0 ||
            monthDropdown.value == 0 ||
            dayDropdown.value == 0)
        {
            error = "태어난 연도와 월, 일을 모두 골라줘!";
            return false;
        }

        birthday = new DateTime(
            GetSelectedYear(),
            monthDropdown.value,
            dayDropdown.value);

        if (birthday > DateTime.Today)
        {
            error = "오늘보다 뒤의 날짜야. 생일을 다시 확인해줄래?";
            return false;
        }

        error = null;
        return true;
    }

    private static void SetOptions(
        TMP_Dropdown dropdown,
        List<string> options,
        int selectedIndex)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.SetValueWithoutNotify(selectedIndex);
        dropdown.RefreshShownValue();
    }
}