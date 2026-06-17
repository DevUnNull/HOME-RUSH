using Fusion;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float playerMoveSpeed = 5f;
    [SerializeField] private float gravity = 3f;

    [Header("Slippery Settings")]
    [SerializeField] private float normalAcceleration = 25f;
    [SerializeField] private float paintAcceleration = 2f;
    [Networked] public bool IsOnPaint { get; set; } = false;

    private CharacterController characterController;
    private PlayerInput inputActions;
    private Vector2 inputVec;

    private Vector3 currentHorizontalVelocity;

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
        HandleMovement(inputVec);
    }

    private void GetInputVector()
    {
        inputVec = inputActions.Player.WASD.ReadValue<Vector2>();
    }

    private void HandleMovement(Vector2 moveVec)
    {
        Vector2 rotatedInput = RotateInputByCamera(moveVec);

        Vector3 targetHorizontalMove = (transform.right * rotatedInput.x + transform.forward * rotatedInput.y).normalized * playerMoveSpeed;

        float currentAcceleration = IsOnPaint ? paintAcceleration : normalAcceleration;

        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetHorizontalMove, Runner.DeltaTime * currentAcceleration);

        Vector3 finalMove = currentHorizontalVelocity + (Vector3.down * gravity);

        characterController.Move(finalMove * Runner.DeltaTime);
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
}
