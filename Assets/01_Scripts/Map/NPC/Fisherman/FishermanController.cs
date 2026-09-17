using UnityEngine;

public class FishermanController : MonoBehaviour
{
    [Header("<< Animator >>")]
    [SerializeField] private Animator animator;

    [Header("<< Fishing Layer >>")]
    [SerializeField] private int fishingLayerIndex = 1;

    private void Start()
    {
        StartFishing();
    }


    public void StartFishing()
    {
        if (animator == null)
            return;

        animator.SetLayerWeight(fishingLayerIndex, 1f);
    }
   
    public void StartInteraction()
    {
        if (animator == null)
            return;

        animator.SetLayerWeight(fishingLayerIndex, 0f);

        LookAtPlayer();
    }

    
    public void EndInteraction()
    {
        if (animator == null)
            return;

        animator.SetLayerWeight(fishingLayerIndex, 1f);
    }

    private void LookAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}