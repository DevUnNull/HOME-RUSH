using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public PlayerMovement playerMovement;
    public PlayerFieldOfView playerFieldOfView;
    public PlayerRotation playerRotation;

    public Entity topPlayerEntity;
    public Entity bottomPlayerEntity;

    public Transform playerHand;
}
