using UnityEngine;

public class Tiger : Animal
{
    protected override void Awake()
    {
        base.Awake();

        maxHp = 150;
        currentHp = maxHp;

        respawnDelay = 5f;

        moveSpeed = 0.6f;
        minIdleTime = 2.5f;
        maxIdleTime = 6f;
        minWalkTime = 1f;
        maxWalkTime = 1f;
        maxRoamDistance = 5f;
    }
}