using UnityEngine;

public class Goat : Animal
{
    protected override void Awake()
    {
        base.Awake();

        maxHp = 100;
        currentHp = maxHp;

        respawnDelay = 4f;

        moveSpeed = 0.1f;
        rotateSpeed = 1f;

        minWalkTime = 1.5f;
        maxWalkTime = 3.5f;

        maxRoamDistance = 4f;
    }

    protected override void StartIdle()
    {
        StartWalk();
    }

    protected override void StartWalk()
    {
        isWalking = true;
        stateTimer = Random.Range(minWalkTime, maxWalkTime);

        moveDirection = GetMoveDirection();

        if (animator != null)
        {
            animator.Play("Goat_Walk");
        }
    }
}
