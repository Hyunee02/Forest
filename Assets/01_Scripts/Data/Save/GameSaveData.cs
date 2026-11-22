using System;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public class PlayerProfileData
{
    public string characterId;
    public string playerName;
}

[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;
    public PlayerProfileData profile = new();

    public int money;
    public int day = 1;
    public int tradeDay = 1;

    // 새 캐릭터는 플레이어 프리팹이 생성된 시점에 인벤토리를 연결한다.
    public List<InventoryItem> inventory;
    public List<ShopDailyRecord> dailyTrades = new();
}

public static class CharacterNameRule
{
    public const int MaxLength = 10;

    public static bool TryValidate(
        string input,
        out string name,
        out string error)
    {
        name = input ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            error = "이름을 입력해주세요.";
            return false;
        }

        foreach (char letter in name)
        {
            if (char.IsWhiteSpace(letter))
            {
                error = "이름에 공백을 사용할 수 없습니다.";
                return false;
            }

            if (char.IsControl(letter))
            {
                error = "이름에 제어 문자를 사용할 수 없습니다.";
                return false;
            }

            bool isKorean =
                (letter >= '\uAC00' && letter <= '\uD7A3') ||
                (letter >= '\u3131' && letter <= '\u3163');

            bool isEnglish =
                (letter >= 'A' && letter <= 'Z') ||
                (letter >= 'a' && letter <= 'z');

            bool isNumber = letter >= '0' && letter <= '9';

            if (!isKorean && !isEnglish && !isNumber)
            {
                error = "이름은 한글, 영문, 숫자만 사용할 수 있습니다.";
                return false;
            }
        }

        int length = new StringInfo(name).LengthInTextElements;

        if (length > MaxLength)
        {
            error = $"이름은 {MaxLength}자 이하로 입력해주세요.";
            return false;
        }

        error = null;
        return true;
    }

    public static class GameSaveValidator
    {
        public static bool Validate(
            GameSaveData data,
            CharacterDatabase database,
            out string error)
        {
            if (data == null || data.profile == null)
            {
                error = "플레이어 데이터가 없습니다.";
                return false;
            }

            if (data.version != GameSaveData.CurrentVersion)
            {
                error = "지원하지 않는 저장 데이터 버전입니다.";
                return false;
            }

            if (database == null)
            {
                error = "캐릭터 데이터베이스가 없습니다.";
                return false;
            }

            if (!database.ValidateDatabase(out error))
                return false;

            if (string.IsNullOrWhiteSpace(data.profile.characterId) ||
                database.GetCharacter(data.profile.characterId) == null)
            {
                error = "저장된 캐릭터를 찾을 수 없습니다.";
                return false;
            }

            if (!CharacterNameRule.TryValidate(
                    data.profile.playerName,
                    out string normalizedName,
                    out error))
            {
                return false;
            }

            data.profile.playerName = normalizedName;
            error = null;
            return true;
        }
    }
}