using UnityEngine;
using Fusion;

public class PlayerRotation : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    private PlayerInput inputActions;
    private Vector2 moveVector;

    public override void Spawned()
    {
        base.Spawned();

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

        GetMoveVecter();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!HasStateAuthority) return;

        HandleRotation(RotateInputByCamera(moveVector));
    }

    private void HandleRotation(Vector2 moveVec)
    {
        if (moveVec == Vector2.zero) return;

        Vector3 targetDirection = new Vector3(moveVec.x, 0f, moveVec.y);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * rotationSpeed);
    }

    private Vector2 RotateInputByCamera(Vector2 input)
    {
        if (CameraManager.Instance == null) return input;

        switch (CameraManager.Instance.CurrentCameraDirection)
        {
            case CameraDirection.Left:

                return new Vector2(input.y, -input.x);

            case CameraDirection.Down:
                return new Vector2(-input.x, -input.y);

            case CameraDirection.Right:
                return new Vector2(-input.y, input.x);

            case CameraDirection.Up:
            default:
                return input;
        }
    }

    private void GetMoveVecter()
    {
        moveVector = inputActions.Player.WASD.ReadValue<Vector2>();
    }
}
