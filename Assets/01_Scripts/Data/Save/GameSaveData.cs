using System;
using System.Globalization;

/// <summary>
/// Ä³¸¯ÅÍ »ı¼º È­¸é ÀÓ½Ã µ¥ÀÌÅÍ
/// </summary>
[Serializable]
public class CharacterCreateDraft
{
    public string characterId;
    public string playerName;
}

/// <summary>
/// Ä³¸¯ÅÍ »ı¼º ¿Ï·á ÈÄ Á¤º¸ º¸°ü
/// </summary>
[Serializable]
public class PlayerProfileData
{
    public string characterId;
    public string playerName;
}

/// <summary>
/// °ÔÀÓ ÀúÀå µ¥ÀÌÅÍ °ü¸®
/// </summary>
[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public PlayerProfileData profile = new();

    public int money;
}

/// <summary>
/// ÇÃ·¹ÀÌ¾î°¡ ÀÔ·ÂÇÑ ÀÌ¸§ÀÌ ±ÔÄ¢¿¡ ¸Â´ÂÁö °Ë»ç
/// </summary>
public static class CharacterNameRule
{
    // ÀÌ¸§ ±æÀÌ Á¦ÇÑ
    public const int MaxLength = 10;

    /// <summary>
    /// ÀÌ¸§ °ËÁõ
    /// </summary>
    /// <param name="input">»ç¿ëÀÚ ÀÔ·Â ÀÌ¸§</param>
    /// <param name="name">ÀÌ¸§ ¹İÈ¯</param>
    /// <param name="error">°ËÁõ ¼º°ø ¿©ºÎ</param>
    /// <returns></returns>
    public static bool TryValidate(string input, out string name, out string error)
    {
        // inputÀÌ nullÀÌ¸é ¿À¸¥ÂÊ °ª »ç¿ë
        name = input ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            error = "ÀÌ¸§À» ÀÔ·ÂÇØÁÖ¼¼¿ä.";
            return false;
        }

        foreach (char letter in name)
        {
            // °ø¹é ¹®ÀÚ °Ë»ç
            if (char.IsWhiteSpace(letter))
            {
                error = "ÀÌ¸§¿¡ °ø¹éÀ» »ç¿ëÇÒ ¼ö ¾ø½À´Ï´Ù.";
                return false;
            }

            // Á¦¾î ¹®ÀÚ °Ë»ç
            if (char.IsControl(letter))
            {
                error = "ÀÌ¸§¿¡ Á¦¾î ¹®ÀÚ¸¦ »ç¿ëÇÒ ¼ö ¾ø½À´Ï´Ù.";
                return false;
            }

            bool isKorean =
                (letter >= '°¡' && letter <= 'ÆR')
                || (letter >= '¤¡' && letter <= '!');

            bool isEnglish =
                (letter >= 'A' && letter <= 'Z')
                || (letter >= 'a' && letter <= 'z');

            bool isNumber = letter >= '0' && letter <= '9';

            // ÇÑ±Û, ¿µ¹®, ¼ıÀÚ¸¸ °¡´É
            if (!isKorean && !isEnglish && !isNumber)
            {
                error = "ÀÌ¸§Àº ÇÑ±Û, ¿µ¹®, ¼ıÀÚ¸¸ »ç¿ëÇÒ ¼ö ÀÖ½À´Ï´Ù.";
                return false;
            }
        }

        // StringInfo : ¹®ÀÚ¿­ ºĞ¼® Å¬·¡½º (new¸¦ »ç¿ëÇØ¼­ nameÀÌ¶ó´Â ¹®ÀÚ¿­À» °¡Áø °´Ã¼ »ı¼º)
        // LengthInTextElements : ¹®ÀÚ¿­¿¡ Æ÷ÇÔµÈ ÅØ½ºÆ® ¿ä¼Ò °³¼ö ¹İÈ¯
        int length = new StringInfo(name).LengthInTextElements;

        if (length > MaxLength)
        {
            error = $"ÀÌ¸§Àº {MaxLength}ÀÚ ÀÌÇÏ·Î ÀÔ·ÂÇØÁÖ¼¼¿ä.";
            return false;
        }

        // ¸ğµç °Ë»ç Åë°ú
        error = null;
        return true;
    }

    public static class GameSaveValidator
    {
        public static bool Validate(GameSaveData data, CharacterDatabase database, out string error)
        {
            if (data == null || data.profile == null)
            {
                error = "ÇÃ·¹ÀÌ¾î µ¥ÀÌÅÍ°¡ ¾ø½À´Ï´Ù.";
                return false;
            }

            if (data.version != GameSaveData.CurrentVersion)
            {
                error = "Áö¿øÇÏÁö ¾Ê´Â ÀúÀå µ¥ÀÌÅÍ ¹öÀüÀÔ´Ï´Ù.";
                return false;
            }

            if (database == null)
            {
                error = "Ä³¸¯ÅÍ µ¥ÀÌÅÍº£ÀÌ½º°¡ ¾ø½À´Ï´Ù.";
                return false;
            }

            if (!database.ValidateDatabase(out error))
                return false;

            if (string.IsNullOrWhiteSpace(data.profile.characterId)
                || database.GetCharacter(data.profile.characterId) == null)
            {
                error = "ÀúÀåµÈ Ä³¸¯ÅÍ¸¦ Ã£À» ¼ö ¾ø½À´Ï´Ù.";
                return false;
            }

            if (!CharacterNameRule.TryValidate(data.profile.playerName, out string normalizedName, out error))
                return false;

            data.profile.playerName = normalizedName;

            error = null;
            return true;
        }
    }
}
