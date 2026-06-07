using Fusion;
using UnityEngine;

public class PlayerHoldTop : State
{
    private Transform target;
    private bool wantThrow = false;

    public PlayerHoldTop(FSM fsm, Entity entity) : base(fsm, entity)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        if (!((PlayerTopEntity)entity).HasStateAuthority) return;
        wantThrow = false;
        target = ((PlayerTopEntity)entity).playerController.playerFieldOfView.visibleOrderedTargets[0];
        ((PlayerTopEntity)entity).PickUpItem(target.GetComponent<NetworkObject>());

        PaintCan paintCan = target.GetComponentInChildren<PaintCan>();
        if (paintCan != null)
        {
            if (paintCan.Object.HasStateAuthority)
            {
                paintCan.hasAlreadySpilled = false;
            }
        }
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (((PlayerTopEntity)entity).inputActions.Player.Throw.WasPressedThisFrame())
        {
            wantThrow = true;
            fsm.ChangeState(((PlayerTopEntity)entity).idleTopState);
            return;
        }

        if (((PlayerTopEntity)entity).inputActions.Player.Hold.WasPressedThisFrame())
        {
            wantThrow = false;
            fsm.ChangeState(((PlayerTopEntity)entity).idleTopState);
            return;
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        if (!((PlayerTopEntity)entity).HasStateAuthority) return;
        
        if (wantThrow)
        {
            ((PlayerTopEntity)entity).ThrowItem(target.GetComponent<NetworkObject>());

        } else
        {
            ((PlayerTopEntity)entity).DropItem(target.GetComponent<NetworkObject>());
        }
    }
}
