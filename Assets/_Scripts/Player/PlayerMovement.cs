using Fusion;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float playerMoveSpeed = 5f;
    [SerializeField] private float gravity = 3f;

    // Biến trạng thái di chuyển để quản lý âm thanh - Tuân
    [Networked] private bool isMoving { get; set; } = false;

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
        Vector3 targetHorizontalMove = (transform.right * moveVec.x + transform.forward * moveVec.y).normalized * playerMoveSpeed;

        float currentAcceleration = IsOnPaint ? paintAcceleration : normalAcceleration;

        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetHorizontalMove, Runner.DeltaTime * currentAcceleration);

        // Thêm điều kiện để xác định khi nào nhân vật đang di chuyển
        bool movingNow = moveVec.magnitude > 0.1f;

        if (movingNow && !isMoving)
        {
            isMoving = true;

            SoundManager.Instance.PlayLoopSFX(
                SoundManager.Instance.sprint
            );
        }
        else if (!movingNow && isMoving)
        {
            isMoving = false;

            SoundManager.Instance.StopLoopSFX();
        }
        //

        Vector3 finalMove = currentHorizontalVelocity + (Vector3.down * gravity);

        characterController.Move(finalMove * Runner.DeltaTime);
    }
}
