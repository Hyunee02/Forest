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
}
