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

        Debug.Log($"{resourceData.resourceName}을(를) 캤습니다.");

        if (resourceData.dropEveryHit)
        {
            TryDropItem();
        }

        if (resourceData.useHp)
        {
            currentHp -= damage;
            currentHp = Mathf.Clamp(currentHp, 0, resourceData.maxHp);

            Debug.Log($"{resourceData.resourceName} 남은 HP: {currentHp}");

            if (currentHp <= 0 && resourceData.destroyWhenHpZero)
            {
                BreakResource();
            }
        }
    }

    private void TryDropItem()
    {
        float totalWeight = resourceData.noDropWeight;

        foreach (ResourceDropData drop in resourceData.drops)
        {
            totalWeight += drop.dropWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < resourceData.noDropWeight)
        {
            Debug.Log("아무것도 획득하지 못했습니다.");
            return;
        }

        randomValue -= resourceData.noDropWeight;

        foreach (ResourceDropData drop in resourceData.drops)
        {
            if (randomValue < drop.dropWeight)
            {
                Debug.Log($"{drop.itemName}을(를) 획득했습니다.");
                return;
            }

            randomValue -= drop.dropWeight;
        }
    }

    private void BreakResource()
    {
        Debug.Log($"{resourceData.resourceName}이(가) 사라졌습니다.");

        gameObject.SetActive(false);
    }

    public bool CanHit(ToolType toolType)
    {
        return toolType == ToolType.Hoe;
    }
}