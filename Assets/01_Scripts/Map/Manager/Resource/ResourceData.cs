using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/Resource Data")]
public class ResourceData : ScriptableObject
{
    [Header("<< 자원 정보 >>")]
    public string resourceName;

    [Header("<< HP 설정 >>")]
    public bool useHp = true;
    public int maxHp = 10;
    public bool destroyWhenHpZero = true;

    [Header("<< 드랍 설정 >>")]
    public bool dropEveryHit = true;

    [Tooltip("아무것도 안 나올 확률 가중치")]
    public float noDropWeight = 75f;

    public ResourceDropData[] drops;
}