using UnityEngine;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerBindInput))]
public class PlayerRest : MonoBehaviour
{
    private PlayerMove playerMove;
    private PlayerBindInput input;
    private Animator animator;

    private RestFurniture currentFurniture;
    private Transform currentExitPoint;

    private bool bRest;

    public bool BRest => bRest;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        input = GetComponent<PlayerBindInput>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        
    }

    public bool Rest(RestFurniture furniture, Transform restPoint, Transform exitPoint, RestPose restPose)
    {
        if (bRest)
            return false;

        if (restPoint == null)
            return false;

        currentFurniture = furniture;
        currentExitPoint = exitPoint;

        bRest = true;

        transform.SetPositionAndRotation(restPoint.position, restPoint.rotation);

        playerMove.SetMoveEnabled(false);

        animator.SetFloat("SpeedZ", 0f);
        animator.SetInteger("RestPose", (int)restPose);
        animator.SetBool("bRest", true);

        return true;
    }

    public void StandUp()
    {
        if (!bRest)
            return;

        bRest = false;

        if (currentExitPoint != null)
            transform.SetPositionAndRotation(currentExitPoint.position, currentExitPoint.rotation);

        animator.SetBool("bRest", false);

        playerMove.SetMoveEnabled(true);

        if (currentFurniture != null)
            currentFurniture.Release(this);

        currentFurniture = null;
        currentExitPoint = null;
    }
}
