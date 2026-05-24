using UnityEngine;

public class PlayerRunBottom : State
{
    public PlayerRunBottom(FSM fsm, Entity entity) : base(fsm, entity)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (((PlayerBottomEntity)entity).inputVector == Vector2.zero)
        {
            fsm.ChangeState(((PlayerBottomEntity)entity).idleBottomState);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
