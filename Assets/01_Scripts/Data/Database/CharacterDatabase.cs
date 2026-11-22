using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Game/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private List<CharacterData> characters = new();

    public IReadOnlyList<CharacterData> Characters => characters;

    public CharacterData GetCharacter(string id)
    {
        return characters.Find(data => data != null && data.ID == id);
    }

    public bool ValidateDatabase(out string error)
    {
        if (characters == null || characters.Count == 0)
        {
            error = "등록된 캐릭터가 없습니다.";
            return false;
        }

        HashSet<string> ids = new();

        foreach (CharacterData data in characters)
        {
            if (data == null)
            {
                error = "캐릭터 목록에 비어 있는 항목이 있습니다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.ID))
            {
                error = "캐릭터 ID가 비어있습니다.";
                return false;
            }

            if (!ids.Add(data.ID))
            {
                error = $"중복된 캐릭터 ID : {data.ID}";
                return false;
            }

            if (data.PreviewPrefab == null || data.PlayerPrefab == null)
            {
                error = $"{data.ID}의 프리팹이 없습니다.";
                return false;
            }
        }

        error = null;
        return true;
    }
}