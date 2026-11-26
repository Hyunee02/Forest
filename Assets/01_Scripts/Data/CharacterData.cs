using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string characterName;
    [SerializeField] private Sprite portrait;

    [FormerlySerializedAs("characterPrefab")]
    [SerializeField] private CharacterView previewPrefab;

    [SerializeField] private GameObject playerPrefab;

    public string ID => id;
    public string CharacterName => characterName;
    public Sprite Portrait => portrait;

    public CharacterView PreviewPrefab => previewPrefab;
    public GameObject PlayerPrefab => playerPrefab;
}