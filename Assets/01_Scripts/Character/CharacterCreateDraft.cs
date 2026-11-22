using System;

[Serializable]
public class CharacterCreateDraft
{
    public string characterId;
    public string playerName;

    public bool TryCreateSaveData(
        CharacterDatabase database,
        out GameSaveData data,
        out string error)
    {
        GameSaveData candidate = new GameSaveData();
        candidate.profile.characterId = characterId;
        candidate.profile.playerName = playerName;

        if (!CharacterNameRule.GameSaveValidator.Validate(
                candidate, database, out error))
        {
            data = null;
            return false;
        }

        data = candidate;
        return true;
    }
}