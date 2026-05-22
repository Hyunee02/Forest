using UnityEngine;

public class HarvestableResource : MonoBehaviour, IHitTarget
{
    [Header("<< 자원 데이터 >>")]
    [SerializeField] private ResourceData resourceData;

    private int currentHp;

    private void Awake()
    {
        if (resourceData == null)
        {
            Debug.LogWarning($"{gameObject.name}에 ResourceData가 없습니다.");
            return;
        }

        currentHp = resourceData.maxHp;
    }

    public void Hit(int damage)
    {
        if (resourceData == null) return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, resourceData.maxHp);

        Debug.Log($"{resourceData.resourceName}을(를) 쳤습니다. 남은 HP: {currentHp}");

        TryDropItem();

        if (currentHp <= 0)
        {
            BreakResource();
        }
    }

    private void TryDropItem()
    {
        if (Random.value <= resourceData.dropChance)
        {
            Debug.Log($"{resourceData.dropItemName}을(를) 획득했습니다.");
        }
    }

    private void BreakResource()
    {
        Debug.Log($"{resourceData.resourceName}이(가) 사라졌습니다.");

        gameObject.SetActive(false);
    }
}