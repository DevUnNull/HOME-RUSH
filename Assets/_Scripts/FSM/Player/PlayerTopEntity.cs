using Fusion;
using UnityEngine;

public class PlayerTopEntity : Entity
{
    public PlayerController playerController;

    public PlayerRunTop runTopState;
    public PlayerIdleTop idleTopState;
    public PlayerHoldTop holdTopState;

    public Vector2 inputVector;
    public PlayerInput inputActions;

    public override void Spawned()
    {
        base.Spawned();

        inputActions = new PlayerInput();
        inputActions.Enable();

        playerController = GetComponent<PlayerController>();
        fsm = new FSM();

        runTopState = new PlayerRunTop(fsm, this);
        idleTopState = new PlayerIdleTop(fsm, this);
        holdTopState = new PlayerHoldTop(fsm, this);

        fsm.Init(idleTopState);
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
