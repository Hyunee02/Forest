using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/Resource Data")]
public class ResourceData : ScriptableObject
{
    [Header("<< 자원 정보 >>")]
    public string resourceName;

    [Header("<< HP >>")]
    public int maxHp = 10;

    [Header("<< 드랍 설정 >>")]
    public string dropItemName;

    [Range(0f, 1f)]
    public float dropChance = 0.25f;
}