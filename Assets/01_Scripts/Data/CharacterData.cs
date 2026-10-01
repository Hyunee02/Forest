using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    private string id;
    private string characterName;

    private Sprite portrait;
    private GameObject characterPrefab;

    public string ID => id;
    public string CharacterName => characterName;
    public Sprite Portrait => portrait;
    public GameObject CharacterPrefab => characterPrefab;
}
