using Fusion;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float playerMoveSpeed = 5f;
    [SerializeField] private float gravity = 3f;

    private CharacterController characterController;
    private PlayerInput inputActions;
    private Vector2 inputVec;

    public override void Spawned()
    {
        base.Spawned();

        characterController = GetComponent<CharacterController>();

        inputActions = new PlayerInput();
        inputActions.Enable();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        inputActions.Disable();
    }

    private void Update()
    {
        if (!HasStateAuthority) return;

        GetInputVector();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        base.FixedUpdateNetwork();
        HandleMovement(inputVec );
    }

    private void GetInputVector()
    {
        inputVec = inputActions.Player.WASD.ReadValue<Vector2>();
    }

    private void HandleMovement(Vector2 moveVec)
    {
        Vector3 move = transform.right * moveVec.x + transform.forward * moveVec.y + Vector3.down * gravity;
        characterController.Move(playerMoveSpeed * Time.fixedDeltaTime * move);
    }
}
