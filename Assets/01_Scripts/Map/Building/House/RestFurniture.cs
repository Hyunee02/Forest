using UnityEngine;

public enum RestPose
{
    ChairSit = 0,
    SofaSit,
    BedLie,
}

public class RestFurniture : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform restPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private RestPose restPose;
    [SerializeField] private string interactionText = "Use";

    private PlayerRest restingPlayer;

    public string InteractionText => interactionText;

    public bool CanInteract(GameObject player)
    {
        PlayerRest playerRest = player.GetComponent<PlayerRest>();

        if (playerRest == null)
            return false;

        return restingPlayer == null || restingPlayer == playerRest;
    }

    public void Interact(GameObject player)
    {
        PlayerRest playerRest = player.GetComponent<PlayerRest>();

        if (playerRest == null)
            return;

        if (restingPlayer == playerRest)
        {
            playerRest.StandUp();
            return;
        }

        if (restingPlayer != null)
            return;

        bool success = playerRest.Rest(this, restPoint, exitPoint, restPose);

        if (success)
            restingPlayer = playerRest;
    }

    public void Release(PlayerRest playerRest)
    {
        if (restingPlayer == playerRest)
            restingPlayer = null;
    }
}