using Fusion;
using UnityEngine;

public class PlayerBottomEntity : Entity
{
    public PlayerController playerController;

    public PlayerRunBottom runBottomState;
    public PlayerIdleBottom idleBottomState;

    public Vector2 inputVector;
    public PlayerInput inputActions;

    public override void Spawned()
    {
        base.Spawned();

        inputActions = new PlayerInput();
        inputActions.Enable();

        playerController = GetComponent<PlayerController>();
        fsm = new FSM();

        runBottomState = new PlayerRunBottom(fsm, this);
        idleBottomState = new PlayerIdleBottom(fsm, this);

        fsm.Init(idleBottomState);
    }

    protected override void Update()
    {
        base.Update();

        if (!HasStateAuthority) return;

        inputVector = inputActions.Player.WASD.ReadValue<Vector2>();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        inputActions.Disable();
    }
}
