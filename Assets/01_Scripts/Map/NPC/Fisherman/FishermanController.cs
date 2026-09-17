using UnityEngine;

public class FishermanController : MonoBehaviour
{
    [Header("<< Animator >>")]
    [SerializeField] private Animator animator;

    [Header("<< Fishing Layer >>")]
    [SerializeField] private int fishingLayerIndex = 1;

    [Header("<< Fishing Rod >>")]
    [SerializeField] private GameObject fishingRod;

    private void Start()
    {
        StartFishing();
    }

    public void StartFishing()
    {
        if (animator == null)
            return;

        animator.SetLayerWeight(fishingLayerIndex, 1f);

        if (fishingRod != null)
        {
            fishingRod.SetActive(true);
        }
    }

    public void StartInteraction()
    {
        if (animator == null)
            return;

        animator.SetLayerWeight(fishingLayerIndex, 0f);

        if (fishingRod != null)
        {
            fishingRod.SetActive(false);
        }
    }

    public void EndInteraction()
    {
        if (animator == null)
            return;

        animator.SetLayerWeight(fishingLayerIndex, 1f);

        if (fishingRod != null)
        {
            fishingRod.SetActive(true);
        }
    }
}