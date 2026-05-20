using UnityEngine;

[CreateAssetMenu(fileName = "ToolVisualData", menuName = "Data/ToolVisualData")]
public class ToolVisualData : ScriptableObject
{
    public int id;
    public Sprite image;
    public GameObject prefab;
}
