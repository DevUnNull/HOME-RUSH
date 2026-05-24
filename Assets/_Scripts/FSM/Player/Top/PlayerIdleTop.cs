using UnityEngine;

public class PlayerIdleTop : State
{
    public PlayerIdleTop(FSM fsm, Entity entity) : base(fsm, entity)
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

        if (((PlayerTopEntity)entity).inputVector != Vector2.zero)
        {
            fsm.ChangeState(((PlayerTopEntity)entity).runTopState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
