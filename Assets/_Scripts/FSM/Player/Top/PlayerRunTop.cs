using UnityEngine;

public class PlayerRunTop : State
{
    public PlayerRunTop(FSM fsm, Entity entity) : base(fsm, entity)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (((PlayerTopEntity)entity).inputActions.Player.Hold.WasPressedThisFrame())
        {
            var targets = ((PlayerTopEntity)entity).playerController.playerFieldOfView.visibleOrderedTargets;
            if (targets != null && targets.Count > 0)
            {
                if (targets[0].CompareTag("Item"))
                {
                    fsm.ChangeState(((PlayerTopEntity)entity).holdTopState);
                    return;
                }
            }

        }

        if (((PlayerTopEntity)entity).inputVector == Vector2.zero)
        {
            fsm.ChangeState(((PlayerTopEntity)entity).idleTopState);
            return;
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
