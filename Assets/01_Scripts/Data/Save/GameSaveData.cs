using System;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 1;

    public int version;
    public CharacterCreateDraft profile;
    public int money;
    public List<InventoryItem> inventoryItems;

    public bool Validate(out string error)
    {
        if (version != CurrentVersion)
        {
            error = $"지원하지 않는 저장 버전입니다. ({version})";
            return false;
        }

        if (profile == null || string.IsNullOrWhiteSpace(profile.characterId))
        {
            error = "저장된 캐릭터 ID가 없습니다.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(profile.playerName) ||
            new StringInfo(profile.playerName).LengthInTextElements > 10)
        {
            error = "저장된 이름이 올바르지 않습니다.";
            return false;
        }

        if (profile.birthYear < 1 || profile.birthYear > 9999 ||
            profile.birthMonth < 1 || profile.birthMonth > 12 ||
            profile.birthDay < 1 ||
            profile.birthDay > DateTime.DaysInMonth(
                profile.birthYear,
                profile.birthMonth))
        {
            error = "저장된 생년월일이 올바르지 않습니다.";
            return false;
        }

        if (money < 0 || inventoryItems == null || inventoryItems.Count == 0)
        {
            error = "저장된 골드 또는 인벤토리가 올바르지 않습니다.";
            return false;
        }

        error = null;
        return true;
    }
}