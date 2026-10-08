using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Game/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private List<CharacterData> characters = new();

    public IReadOnlyList<CharacterData> Characters => characters;

    /// <summary>
    /// ID 일치하는 캐릭터 데이터 검색
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharacterData GetCharacter(string id)
    {
        return characters.Find(data => data != null && data.ID == id);
    }

    /// <summary>
    /// 데이터베이스 검사
    /// </summary>
    /// <param name="error">검증 성공 여부</param>
    /// <returns></returns>
    public bool ValidateDatabase(out string error)
    {
        if (characters == null || characters.Count == 0)
        {
            error = "등록된 캐릭터가 없습니다.";
            return false;
        }

        HashSet<string> ids = new();

        // 캐릭터 검사
        foreach (CharacterData data in characters)
        {
            if (data == null)
            {
                error = "캐릭터 목록에 비어 있는 항목이 있습니다.";
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

            if (data.CharacterPrefab == null)
            {
                error = $"{data.ID}의 프리팹이 없습니다.";
                return false;
            }

            //if (data.CharacterPrefab.Animator == null
            //    || data.CharacterPrefab.HandSocket == null)
            //{
            //    error = $"{data.ID}의 Animator 또는 HandSocket이 없습니다.";
            //    return false;
            //}
        }

        error = null;
        return true;
    }
}
