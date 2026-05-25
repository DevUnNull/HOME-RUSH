using UnityEngine;

public class PlayerIdleBottom : State
{
    public PlayerIdleBottom(FSM fsm, Entity entity) : base(fsm, entity)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (((PlayerBottomEntity)entity).inputVector != Vector2.zero)
        {
            fsm.ChangeState(((PlayerBottomEntity)entity).runBottomState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
