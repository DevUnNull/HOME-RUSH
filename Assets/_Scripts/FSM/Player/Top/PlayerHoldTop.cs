using Fusion;
using UnityEngine;

public class PlayerHoldTop : State
{
    private Transform target;

    public PlayerHoldTop(FSM fsm, Entity entity) : base(fsm, entity)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        if (!((PlayerTopEntity)entity).HasStateAuthority) return;

        target = ((PlayerTopEntity)entity).playerController.playerFieldOfView.visibleOrderedTargets[0];
        ((PlayerTopEntity)entity).PickUpItem(target.GetComponent<NetworkObject>());
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (((PlayerTopEntity)entity).inputActions.Player.Hold.WasPressedThisFrame())
        {
            fsm.ChangeState(((PlayerTopEntity)entity).idleTopState);
            return;
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        if (!((PlayerTopEntity)entity).HasStateAuthority) return;
        ((PlayerTopEntity)entity).DropItem(target.GetComponent<NetworkObject>());
    }
}
