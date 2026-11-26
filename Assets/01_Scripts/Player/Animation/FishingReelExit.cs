using UnityEngine;

public class FishingReelExit : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        PlayerFishing fishing = animator.GetComponentInParent<PlayerFishing>();

        if (fishing != null)
            fishing.CompleteReel();
    }
}